using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.API.Models.Infra;
using EatTogether.API.Models.Repositories;
using EatTogether.API.Models.ViewModels;
using Google.Apis.Auth;
using System.Text.Json;

namespace EatTogether.API.Models.Services
{
	public interface IAuthService
	{
		// -----內用點餐頁用------------------------------
		Task<Result<MemberLoginDto>> MemberLoginAsync(string email, string password);

		// -----前台一般登入用------------------------------
		Task<Result<MemberViewModel>> LoginAsync(LoginDto dto);

		// -----前台登出用------------------------------
		Task LogoutAsync(HttpRequest request);

		// -----前台 Refresh Token 用------------------------------
		Task<Result<MemberViewModel>> RefreshTokenAsync(string refreshToken);

		// -----前台復原帳號用------------------------------
		Task<Result<MemberViewModel>> RestoreAccountAsync(LoginDto dto);

		// -----前台會員註冊用------------------------------
		Task<Result> RegisterAsync(RegisterDto dto);

		// -----前台 Email 驗證用------------------------------
		Task<Result> VerifyEmailAsync(string token);
		Task<Result> ResendVerifyEmailAsync(string email);

		// -----前台忘記密碼用------------------------------
		Task<Result> ForgotPasswordAsync(string email);

		// -----前台重設密碼用------------------------------
		Task<Result> ValidateResetTokenAsync(string token);
		Task<Result> ResetPasswordAsync(ResetPasswordDto dto);

		// -----前台 Google OAuth 用------------------------------
		Task<Result<MemberViewModel>> GoogleCallbackAsync(string code);
	}

	public class AuthService : IAuthService
	{
		// Google OAuth token exchange 共用同一個 HttpClient（避免 socket exhaustion）
		private static readonly HttpClient _httpClient = new();

		private readonly IHttpContextAccessor _httpContextAccessor;

		// -----內用點餐頁用------------------------------
		private readonly IMemberRepository _memberRepo;

		// -----前台會員註冊用------------------------------
		private readonly IEmailService _emailService;
		private readonly IConfiguration _config;

		// -----前台一般登入用------------------------------
		private readonly JwtHelper _jwtHelper;
		private readonly IWebHostEnvironment _env;

		public AuthService(
			IHttpContextAccessor httpContextAccessor,
			IMemberRepository memberRepo,
			IEmailService emailService,
			IConfiguration config,
			JwtHelper jwtHelper,
			IWebHostEnvironment env)
		{
			_httpContextAccessor = httpContextAccessor;
			_memberRepo = memberRepo;
			_emailService = emailService;
			_config = config;
			_jwtHelper = jwtHelper;
			_env = env;
		}

		// -----前台一般登入用------------------------------
		public async Task<Result<MemberViewModel>> LoginAsync(LoginDto dto)
		{
			// 1. 以 Account 查詢（含 MemberExternalLogins，供判斷 googleLinked；含軟刪除會員）
			var member = await _memberRepo.GetMemberByAccountAsync(dto.Account);

			// 2. 找不到 → 統一錯誤（不透露帳號是否存在）
			if (member == null)
				return Result<MemberViewModel>.Fail("account_or_password_error");

			// 3. 純 Google 帳號 → 統一錯誤
			if (member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
				return Result<MemberViewModel>.Fail("account_or_password_error");

			// 4. BCrypt 驗證密碼失敗 → 統一錯誤
			if (!HashUtility.VerifyPassword(dto.Password, member.HashedPassword))
				return Result<MemberViewModel>.Fail("account_or_password_error");

			// 5. 已軟刪除 → 密碼驗證通過後才揭露，供前端顯示復原按鈕
			if (member.IsDeleted)
				return Result<MemberViewModel>.Fail("account_deleted");

			// 6. Email 未驗證
			if (!member.IsConfirmed)
				return Result<MemberViewModel>.Fail("email_not_verified");

			// 7. 帳號已停權
			if (member.IsBlacklisted)
				return Result<MemberViewModel>.Fail("account_blacklisted");

			// 8. 簽發 JWT
			var accessToken = _jwtHelper.GenerateAccessToken(member.Id, member.Name);
			var refreshToken = _jwtHelper.GenerateRefreshToken();

			// 9. 儲存 RefreshToken 至 MemberRefreshTokens
			var refreshTokenExpireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"]!);
			await _memberRepo.SaveRefreshTokenAsync(new MemberRefreshToken
			{
				MemberId = member.Id,
				Token = refreshToken,
				ExpiresAt = DateTime.Now.AddDays(refreshTokenExpireDays),
				IsRevoked = false,
				CreatedAt = DateTime.Now,
			});

			// 10. 寫入 Cookie
			var response = _httpContextAccessor.HttpContext!.Response;
			CookieHelper.SetAccessTokenCookie(response, accessToken, _env);
			CookieHelper.SetRefreshTokenCookie(response, refreshToken, _env);

			// 11. 回傳 MemberViewModel
			var googleLogin = member.MemberExternalLogins
				.FirstOrDefault(e => e.Provider == "google");

			return Result<MemberViewModel>.Success(new MemberViewModel
			{
				Id = member.Id,
				Name = member.Name,
				Email = member.Email,
				AvatarFileName = member.AvatarFileName,
				GoogleAvatarUrl = googleLogin?.AvatarUrl,
				HashedPasswordStatus = member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD
					? "EXTERNAL_LOGIN_NO_PASSWORD"
					: "HAS_PASSWORD",
				GoogleLinked = googleLogin != null,
			});
		}

		// -----前台登出用------------------------------
		public async Task LogoutAsync(HttpRequest request)
		{
			// 1. 從 Cookie 取出 refresh_token，有值才撤銷（防止帳號枚舉）
			var refreshToken = request.Cookies["refresh_token"];
			if (!string.IsNullOrEmpty(refreshToken))
				await _memberRepo.RevokeRefreshTokenByTokenStringAsync(refreshToken);

			// 2. 清除前端的兩個 Cookie
			var response = _httpContextAccessor.HttpContext!.Response;
			CookieHelper.ClearAuthCookies(response, _env);
		}

		// -----前台 Refresh Token 用------------------------------
		public async Task<Result<MemberViewModel>> RefreshTokenAsync(string refreshToken)
		{
			// 1. 查詢 MemberRefreshTokens（含 Member + MemberExternalLogins）
			var tokenRecord = await _memberRepo.GetRefreshTokenAsync(refreshToken);

			// 2. 找不到 → 401
			if (tokenRecord == null)
				return Result<MemberViewModel>.Fail("invalid_refresh_token");

			// 3. 已撤銷 → 401
			if (tokenRecord.IsRevoked)
				return Result<MemberViewModel>.Fail("invalid_refresh_token");

			// 4. 已過期 → 401
			if (tokenRecord.ExpiresAt <= DateTime.Now)
				return Result<MemberViewModel>.Fail("invalid_refresh_token");

			// 5. 舊 token 標記 IsRevoked=true（Token Rotation）
			await _memberRepo.RevokeRefreshTokenAsync(tokenRecord.Id);

			// 6. 產生新 Access Token + 新 Refresh Token
			var member = tokenRecord.Member;
			var newAccessToken = _jwtHelper.GenerateAccessToken(member.Id, member.Name);
			var newRefreshToken = _jwtHelper.GenerateRefreshToken();

			// 7. 儲存新 Refresh Token
			var refreshTokenExpireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"]!);
			await _memberRepo.SaveRefreshTokenAsync(new MemberRefreshToken
			{
				MemberId = member.Id,
				Token = newRefreshToken,
				ExpiresAt = DateTime.Now.AddDays(refreshTokenExpireDays),
				IsRevoked = false,
				CreatedAt = DateTime.Now,
			});

			// 8. 寫入新 Cookie
			var response = _httpContextAccessor.HttpContext!.Response;
			CookieHelper.SetAccessTokenCookie(response, newAccessToken, _env);
			CookieHelper.SetRefreshTokenCookie(response, newRefreshToken, _env);

			// 9. 回傳 MemberViewModel
			var googleLogin = member.MemberExternalLogins
				.FirstOrDefault(e => e.Provider == "google");

			return Result<MemberViewModel>.Success(new MemberViewModel
			{
				Id = member.Id,
				Name = member.Name,
				Email = member.Email,
				AvatarFileName = member.AvatarFileName,
				GoogleAvatarUrl = googleLogin?.AvatarUrl,
				HashedPasswordStatus = member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD
					? "EXTERNAL_LOGIN_NO_PASSWORD"
					: "HAS_PASSWORD",
				GoogleLinked = googleLogin != null,
			});
		}

		// -----前台復原帳號用------------------------------
		public async Task<Result<MemberViewModel>> RestoreAccountAsync(LoginDto dto)
		{
			// 1. 以 Account 查詢（含軟刪除會員）
			var member = await _memberRepo.GetMemberByAccountAsync(dto.Account);

			// 2. 找不到 → 統一錯誤
			if (member == null)
				return Result<MemberViewModel>.Fail("account_or_password_error");

			// 3. 純 Google 帳號 → 統一錯誤
			if (member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
				return Result<MemberViewModel>.Fail("account_or_password_error");

			// 4. BCrypt 驗證密碼失敗 → 統一錯誤
			if (!HashUtility.VerifyPassword(dto.Password, member.HashedPassword))
				return Result<MemberViewModel>.Fail("account_or_password_error");

			// 5. 帳號未刪除 → 不需復原
			if (!member.IsDeleted)
				return Result<MemberViewModel>.Fail("account_not_deleted");

			// 6. 帳號已停權
			if (member.IsBlacklisted)
				return Result<MemberViewModel>.Fail("account_blacklisted");

			// 7. 復原帳號（IsDeleted=0、DeletedAt=NULL）
			await _memberRepo.RestoreAccountAsync(member.Id);

			// 8. 簽發 JWT
			var accessToken = _jwtHelper.GenerateAccessToken(member.Id, member.Name);
			var refreshToken = _jwtHelper.GenerateRefreshToken();

			// 9. 儲存 RefreshToken
			var refreshTokenExpireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"]!);
			await _memberRepo.SaveRefreshTokenAsync(new MemberRefreshToken
			{
				MemberId = member.Id,
				Token = refreshToken,
				ExpiresAt = DateTime.Now.AddDays(refreshTokenExpireDays),
				IsRevoked = false,
				CreatedAt = DateTime.Now,
			});

			// 10. 寫入 Cookie
			var response = _httpContextAccessor.HttpContext!.Response;
			CookieHelper.SetAccessTokenCookie(response, accessToken, _env);
			CookieHelper.SetRefreshTokenCookie(response, refreshToken, _env);

			// 11. 回傳 MemberViewModel
			var googleLogin = member.MemberExternalLogins
				.FirstOrDefault(e => e.Provider == "google");

			return Result<MemberViewModel>.Success(new MemberViewModel
			{
				Id = member.Id,
				Name = member.Name,
				Email = member.Email,
				AvatarFileName = member.AvatarFileName,
				GoogleAvatarUrl = googleLogin?.AvatarUrl,
				HashedPasswordStatus = "HAS_PASSWORD",
				GoogleLinked = googleLogin != null,
			});
		}

		// -----前台會員註冊用------------------------------
		public async Task<Result> RegisterAsync(RegisterDto dto)
		{
			// 1. 帳號唯一性檢查
			if (await _memberRepo.IsAccountExistsAsync(dto.Account))
				return Result.Fail("account_taken");

			// 2. Email 唯一性檢查
			if (await _memberRepo.IsEmailExistsAsync(dto.Email))
				return Result.Fail("email_taken");

			// 3. BCrypt Hash
			var hashedPassword = HashUtility.HashPassword(dto.Password);

			// 4. 建立 Member（IsConfirmed=0，待 Email 驗證後開通）
			var member = new Member
			{
				Account = dto.Account,
				Name = dto.Name,
				Email = dto.Email,
				HashedPassword = hashedPassword,
				IsConfirmed = false,
				IsBlacklisted = false,
				IsDeleted = false,
				CreatedAt = DateTime.Now
			};
			await _memberRepo.CreateMemberAsync(member);

			// 5. 寄信保護：同一 MemberId 若已有未過期且未使用的 token，不重建
			//    ← 修正：這裡回傳 Success，因為帳號已建立成功，只是不重複寄信
			if (await _memberRepo.HasPendingConfirmTokenAsync(member.Id))
				return Result.Success();

			// 6. 建立 MemberConfirmToken（有效期 15 分鐘）
			var tokenString = Guid.NewGuid().ToString("N");
			var confirmToken = new MemberConfirmToken
			{
				MemberId = member.Id,
				Token = tokenString,
				NewEmail = null,
				ExpiresAt = DateTime.Now.AddMinutes(15),
				IsUsed = false,
				CreatedAt = DateTime.Now
			};
			await _memberRepo.CreateConfirmTokenAsync(confirmToken);

			// 7. 寄出 Email 驗證信
			var frontendBaseUrl = _config["FrontendBaseUrl"];
			var verifyUrl = $"{frontendBaseUrl}/verify-email?token={tokenString}";
			await _emailService.SendVerifyEmailAsync(dto.Email, verifyUrl);

			return Result.Success();
		}

		// -----前台 Email 驗證用------------------------------

		public async Task<Result> VerifyEmailAsync(string token)
		{
			var confirmToken = await _memberRepo.GetConfirmTokenAsync(token);

			if (confirmToken == null || confirmToken.IsUsed || confirmToken.ExpiresAt <= DateTime.Now)
				return Result.Fail("token_invalid");

			// 確認 Member 未被刪除或列入黑名單
			var member = await _memberRepo.GetMemberByIdAsync(confirmToken.MemberId);
			if (member == null || member.IsDeleted || member.IsBlacklisted)
				return Result.Fail("token_invalid");

			if (confirmToken.NewEmail == null)
				await _memberRepo.ConfirmMemberAndMarkTokenUsedAsync(confirmToken.MemberId, confirmToken.Id);
			else
				await _memberRepo.UpdateEmailAndMarkTokenUsedAsync(confirmToken.MemberId, confirmToken.NewEmail, confirmToken.Id);

			return Result.Success();
		}

		public async Task<Result> ResendVerifyEmailAsync(string email)
		{
			// 1. 找不到 → 靜默回傳成功（防枚舉）
			var member = await _memberRepo.GetByEmailAsync(email);
			if (member == null)
				return Result.Success();

			// 2. IsConfirmed=1 → 回傳 already_confirmed
			if (member.IsConfirmed)
				return Result.Fail("already_confirmed");

			// 3. 將舊未使用 token 標記已使用，建立新 token，寄信
			await _memberRepo.MarkOldConfirmTokensUsedAsync(member.Id);

			var tokenString = Guid.NewGuid().ToString("N");
			var confirmToken = new MemberConfirmToken
			{
				MemberId = member.Id,
				Token = tokenString,
				NewEmail = null,
				ExpiresAt = DateTime.Now.AddMinutes(15),
				IsUsed = false,
				CreatedAt = DateTime.Now
			};
			await _memberRepo.CreateConfirmTokenAsync(confirmToken);

			var frontendBaseUrl = _config["FrontendBaseUrl"];
			var verifyUrl = $"{frontendBaseUrl}/verify-email?token={tokenString}";
			await _emailService.SendVerifyEmailAsync(member.Email, verifyUrl);

			return Result.Success();
		}

		// -----前台忘記密碼用------------------------------
		public async Task<Result> ForgotPasswordAsync(string email)
		{
			// 1. 以 Email 查詢（不含軟刪除）
			//    找不到 / 已刪除 / 已停權 / 未驗證 → 全部靜默回傳 Success（防枚舉）
			var member = await _memberRepo.GetByEmailAsync(email);
			if (member == null || member.IsDeleted || member.IsBlacklisted || !member.IsConfirmed)
				return Result.Success();

			// 2. 寄信保護：已有未過期且未使用的 token → 靜默回傳 Success（防濫寄）
			if (await _memberRepo.HasPendingPasswordResetTokenAsync(member.Id))
				return Result.Success();

			// 3. 建立 MemberPasswordResetToken（有效期 30 分鐘）
			var tokenString = Guid.NewGuid().ToString("N");
			var resetToken = new MemberPasswordResetToken
			{
				MemberId = member.Id,
				Token = tokenString,
				ExpiresAt = DateTime.Now.AddMinutes(30),
				IsUsed = false,
				CreatedAt = DateTime.Now,
			};
			await _memberRepo.CreatePasswordResetTokenAsync(resetToken);

			// 4. 寄出密碼重設信
			var frontendBaseUrl = _config["FrontendBaseUrl"];
			var resetUrl = $"{frontendBaseUrl}/reset-password?token={tokenString}";
			await _emailService.SendPasswordResetEmailAsync(member.Email, resetUrl);

			return Result.Success();
		}

		// -----前台重設密碼用------------------------------
		public async Task<Result> ValidateResetTokenAsync(string token)
		{
			var tokenRecord = await _memberRepo.GetPasswordResetTokenAsync(token);

			if (tokenRecord == null || tokenRecord.IsUsed || tokenRecord.ExpiresAt <= DateTime.Now)
				return Result.Fail("token_invalid");

			// 確認 Member 狀態正常
			var member = tokenRecord.Member;
			if (member == null || member.IsDeleted || member.IsBlacklisted)
				return Result.Fail("token_invalid");

			return Result.Success();
		}

		public async Task<Result> ResetPasswordAsync(ResetPasswordDto dto)
		{
			// 1. 驗證 token（與 ValidateResetTokenAsync 邏輯一致）
			var tokenRecord = await _memberRepo.GetPasswordResetTokenAsync(dto.Token);

			if (tokenRecord == null || tokenRecord.IsUsed || tokenRecord.ExpiresAt <= DateTime.Now)
				return Result.Fail("token_invalid");

			var member = tokenRecord.Member;
			if (member == null || member.IsDeleted || member.IsBlacklisted)
				return Result.Fail("token_invalid");

			// 2. BCrypt Hash 新密碼
			var hashedPassword = HashUtility.HashPassword(dto.NewPassword);

			// 3. 更新密碼
			await _memberRepo.UpdateMemberPasswordAsync(member.Id, hashedPassword);

			// 4. 標記 token 已使用
			await _memberRepo.MarkPasswordResetTokenUsedAsync(tokenRecord.Id);

			// 5. 撤銷該會員所有 RefreshToken（密碼更新後踢除所有裝置）
			await _memberRepo.RevokeAllRefreshTokensByMemberIdAsync(member.Id);

			return Result.Success();
		}

		// -----前台 Google OAuth 用------------------------------
		public async Task<Result<MemberViewModel>> GoogleCallbackAsync(string code)
		{
			// 1. 用 code 向 Google 換取 id_token
			var clientId = _config["Google:ClientId"]!;
			var clientSecret = _config["Google:ClientSecret"]!;
			var redirectUri = _config["Google:RedirectUri"]!;

			var tokenResponse = await _httpClient.PostAsync(
				"https://oauth2.googleapis.com/token",
				new FormUrlEncodedContent(new Dictionary<string, string>
				{
					["code"] = code,
					["client_id"] = clientId,
					["client_secret"] = clientSecret,
					["redirect_uri"] = redirectUri,
					["grant_type"] = "authorization_code",
				}));

			if (!tokenResponse.IsSuccessStatusCode)
				return Result<MemberViewModel>.Fail("google_auth_failed");

			var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
			using var doc = JsonDocument.Parse(tokenJson);
			if (!doc.RootElement.TryGetProperty("id_token", out var idTokenElement))
				return Result<MemberViewModel>.Fail("google_auth_failed");

			var idToken = idTokenElement.GetString()!;

			// 2. 驗證 id_token，解析 sub、email、name、avatarUrl
			GoogleJsonWebSignature.Payload payload;
			try
			{
				payload = await GoogleJsonWebSignature.ValidateAsync(
					idToken,
					new GoogleJsonWebSignature.ValidationSettings { Audience = new[] { clientId } });
			}
			catch
			{
				return Result<MemberViewModel>.Fail("google_auth_failed");
			}

			var providerUserId = payload.Subject;
			var email = payload.Email;
			var name = payload.Name ?? email.Split('@')[0];
			var avatarUrl = payload.Picture;

			// 3. 查詢 MemberExternalLogins WHERE Provider='google' AND ProviderUserId=sub
			Member member;
			var externalLogin = await _memberRepo.GetExternalLoginByProviderAsync("google", providerUserId);

			if (externalLogin != null)
			{
				// 找到：使用現有 Member（Include 在 Repository 已處理）
				if (externalLogin.Member == null)
					return Result<MemberViewModel>.Fail("login_failed");
				member = externalLogin.Member;
			}
			else
			{
				// 4. 查詢 Members.Email（GetByEmailAsync 已過濾 IsDeleted=1）
				var existingMember = await _memberRepo.GetByEmailAsync(email);

				if (existingMember != null)
				{
					// 停權帳號不允許連結
					if (existingMember.IsBlacklisted)
						return Result<MemberViewModel>.Fail("account_blacklisted");

					// 一般帳號（有密碼）不允許透過第三方登入自動綁定
					// 需登入後在會員中心手動點「前往連結」才能綁定
					if (existingMember.HashedPassword != HashUtility.EXTERNAL_LOGIN_NO_PASSWORD)
						return Result<MemberViewModel>.Fail("account_exists_use_password");

					// 未驗證帳號視為 Google 已驗證，自動設為已驗證
					if (!existingMember.IsConfirmed)
					{
						existingMember.IsConfirmed = true;
						await _memberRepo.SetMemberConfirmedAsync(existingMember.Id);
					}

					// 5. Email 已存在（IsDeleted=0）→ 自動連結，寄安全通知信
					member = existingMember;
					await _memberRepo.CreateExternalLoginAsync(new MemberExternalLogin
					{
						MemberId = member.Id,
						Provider = "google",
						ProviderUserId = providerUserId,
						AvatarUrl = avatarUrl,
						ProviderEmail = email,
						CreatedAt = DateTime.Now,
					});
					await _emailService.SendSecurityNoticeAsync(
						member.Email,
						"您的帳號已新增 Google 登入，若非本人操作請立即修改密碼");
				}
				else
				{
					// 6. Email 不存在（或已軟刪除）→ 建立新 Member
					var newMember = new Member
					{
						Account = $"google_{providerUserId}",
						Name = name,
						Email = email,
						HashedPassword = HashUtility.EXTERNAL_LOGIN_NO_PASSWORD,
						IsConfirmed = true,
						IsBlacklisted = false,
						IsDeleted = false,
						CreatedAt = DateTime.Now,
					};
					await _memberRepo.CreateMemberAsync(newMember);

					await _memberRepo.CreateExternalLoginAsync(new MemberExternalLogin
					{
						MemberId = newMember.Id,
						Provider = "google",
						ProviderUserId = providerUserId,
						AvatarUrl = avatarUrl,
						ProviderEmail = email,
						CreatedAt = DateTime.Now,
					});
					member = newMember;
				}
			}

			// 7. 已軟刪除 → 純 Google 帳號無法用帳密恢復，透過重新 OAuth 登入自動恢復
			if (member.IsDeleted)
				await _memberRepo.RestoreAccountAsync(member.Id);

			// 8. 已停權
			if (member.IsBlacklisted)
				return Result<MemberViewModel>.Fail("account_blacklisted");

			// 9. 簽發 JWT + Refresh Token，寫入 Cookie
			var accessToken = _jwtHelper.GenerateAccessToken(member.Id, member.Name);
			var refreshToken = _jwtHelper.GenerateRefreshToken();

			var refreshTokenExpireDays = int.Parse(_config["Jwt:RefreshTokenExpireDays"]!);
			await _memberRepo.SaveRefreshTokenAsync(new MemberRefreshToken
			{
				MemberId = member.Id,
				Token = refreshToken,
				ExpiresAt = DateTime.Now.AddDays(refreshTokenExpireDays),
				IsRevoked = false,
				CreatedAt = DateTime.Now,
			});

			var response = _httpContextAccessor.HttpContext!.Response;
			CookieHelper.SetAccessTokenCookie(response, accessToken, _env);
			CookieHelper.SetRefreshTokenCookie(response, refreshToken, _env);

			// 10. 回傳 MemberViewModel（Google OAuth 後 GoogleLinked 必為 true）
			return Result<MemberViewModel>.Success(new MemberViewModel
			{
				Id = member.Id,
				Account = member.Account,
				Name = member.Name,
				Email = member.Email,
				AvatarFileName = member.AvatarFileName,
				GoogleAvatarUrl = avatarUrl,
				HashedPasswordStatus = member.HashedPassword == HashUtility.EXTERNAL_LOGIN_NO_PASSWORD
					? "EXTERNAL_LOGIN_NO_PASSWORD"
					: "HAS_PASSWORD",
				GoogleLinked = true,
			});
		}

		// -----內用點餐頁用------------------------------
		public async Task<Result<MemberLoginDto>> MemberLoginAsync(string email, string password)
		{
			var member = await _memberRepo.GetByEmailAsync(email);

			if (member == null)
				return Result<MemberLoginDto>.Fail($"找不到email: {email}");

			if (!HashUtility.VerifyPassword(password, member.HashedPassword))
				return Result<MemberLoginDto>.Fail($"密碼錯誤");

			if (member.IsBlacklisted)
				return Result<MemberLoginDto>.Fail("黑名單");

			if (!member.IsConfirmed)
				return Result<MemberLoginDto>.Fail("未驗證");

			return Result<MemberLoginDto>.Success(new MemberLoginDto
			{
				MemberId = member.Id,
				Name = member.Name,
				Email = member.Email,
			});
		}
	}
}
