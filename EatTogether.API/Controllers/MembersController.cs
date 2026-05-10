using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.Services;
using EatTogether.API.Models.ViewModels.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EatTogether.API.Controllers
{
	[Route("api/members")]
	[ApiController]
	public class MembersController : ControllerBase
	{
		private readonly IMemberService _memberService;

		public MembersController(IMemberService memberService)
		{
			_memberService = memberService;
		}

		// GET /api/members/me
		[HttpGet("me")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> GetMe()
		{
			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.GetMeAsync(memberId);

			if (!result.IsSuccess)
				return NotFound(new ErrorViewModel
				{
					Message = "找不到會員",
					ErrorCode = result.ErrorMessage
				});

			return Ok(result.Value);
		}

		// PUT /api/members/me/profile
		[HttpPut("me/profile")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.UpdateProfileAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"invalid_birth_date" => BadRequest(new ErrorViewModel
					{
						Message = "生日不可為未來日期",
						ErrorCode = "invalid_birth_date"
					}),
					"birth_date_locked" => BadRequest(new ErrorViewModel
					{
						Message = "生日設定後無法修改",
						ErrorCode = "birth_date_locked"
					}),
					"update_failed" => BadRequest(new ErrorViewModel
					{
						Message = "更新失敗",
						ErrorCode = "update_failed"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "更新失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "個人資料已更新" });
		}

		// POST /api/members/me/avatar
		[HttpPost("me/avatar")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		[RequestSizeLimit(3 * 1024 * 1024)] // 3MB 上限，Service 層再驗證 2MB
		public async Task<IActionResult> UploadAvatar(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new ErrorViewModel
				{
					Message = "請選擇要上傳的圖片",
					ErrorCode = "no_file"
				});

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.UploadAvatarAsync(memberId, file);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"invalid_format" => BadRequest(new ErrorViewModel
					{
						Message = "僅支援 JPG、PNG、WebP 格式",
						ErrorCode = "invalid_format"
					}),
					"file_too_large" => BadRequest(new ErrorViewModel
					{
						Message = "圖片大小不可超過 2MB",
						ErrorCode = "file_too_large"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "上傳失敗，請稍後再試" })
				};
			}

			return Ok(new { avatarFileName = result.Value });
		}

		// PUT /api/members/me/password
		[HttpPut("me/password")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.ChangePasswordAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"no_password" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號使用 Google 登入，無法修改密碼",
						ErrorCode = "no_password"
					}),
					"wrong_password" => BadRequest(new ErrorViewModel
					{
						Message = "目前密碼不正確",
						ErrorCode = "wrong_password"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "修改失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "密碼已成功修改" });
		}

		// POST /api/members/me/email
		[HttpPost("me/email")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> RequestEmailChange([FromBody] RequestEmailChangeDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.RequestEmailChangeAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"same_email" => BadRequest(new ErrorViewModel
					{
						Message = "新 Email 與目前相同",
						ErrorCode = "same_email"
					}),
					"email_taken" => Conflict(new ErrorViewModel
					{
						Message = "此 Email 已被其他帳號使用",
						ErrorCode = "email_taken"
					}),
					"cannot_change_email_external_login" => BadRequest(new ErrorViewModel
					{
						Message = "第三方登入帳號的 Email 無法修改",
						ErrorCode = "cannot_change_email_external_login"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "申請失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "驗證信已寄至新信箱，請點擊連結完成變更" });
		}

		// POST /api/members/me/create-account
		[HttpPost("me/create-account")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.CreateAccountAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"already_has_account" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已設有一般帳號密碼",
						ErrorCode = "already_has_account"
					}),
					"account_taken" => Conflict(new ErrorViewModel
					{
						Message = "帳號已被使用，請換一個",
						ErrorCode = "account_taken"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "建立失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "帳號已建立，往後可使用帳號密碼登入" });
		}

		// DELETE /api/members/me/google-link
		[HttpDelete("me/google-link")]
		[Authorize]
		[EnableRateLimiting("GeneralPolicy")]
		public async Task<IActionResult> UnlinkGoogle()
		{
			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.UnlinkGoogleAsync(memberId);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"cannot_unlink_only_login" => BadRequest(new ErrorViewModel
					{
						Message = "請先建立帳號密碼後再取消第三方連結",
						ErrorCode = "cannot_unlink_only_login"
					}),
					"google_not_linked" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號未連結 Google",
						ErrorCode = "google_not_linked"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "取消連結失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel { Message = "已取消與 Google 帳號的連結" });
		}

		// 會員中心連結 Google
		// POST /api/members/me/google-link
		[HttpPost("me/google-link")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> LinkGoogle([FromBody] GoogleCallbackDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

			var result = await _memberService.LinkGoogleAsync(memberId, dto.Code);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => StatusCode(403, new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"email_not_confirmed" => BadRequest(new ErrorViewModel
					{
						Message = "請先驗證您的信箱",
						ErrorCode = "email_not_confirmed"
					}),
					"google_already_linked_to_other" => Conflict(new ErrorViewModel
					{
						Message = "此 Google 帳號已被其他帳號綁定",
						ErrorCode = "google_already_linked_to_other"
					}),
					"google_auth_failed" => BadRequest(new ErrorViewModel
					{
						Message = "Google 驗證失敗，請重試",
						ErrorCode = "google_auth_failed"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "連結失敗，請稍後再試" })
				};
			}

			// 只回傳成功，絕不簽發或更新 JWT Cookie
			return Ok(new SuccessViewModel { Message = "Google 帳號已成功連結" });
		}

		/// <summary>
		/// 申請刪除帳號 (第一步)
		/// </summary>
		// POST /api/members/me/delete-request
		[HttpPost("me/delete-request")]
		[Authorize]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> RequestDeleteAccount([FromBody] DeleteAccountDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			int memberId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var result = await _memberService.RequestDeleteAccountAsync(memberId, dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"password_required" => BadRequest(new ErrorViewModel
					{
						Message = "請輸入密碼",
						ErrorCode = "password_required"
					}),
					"wrong_password" => BadRequest(new ErrorViewModel
					{
						Message = "密碼不正確",
						ErrorCode = "wrong_password"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "申請失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel
			{
				Message = "確認信已寄至您的信箱，請點擊連結完成刪除"
			});
		}

		/// <summary>
		/// 確認刪除帳號 (第二步,點擊信中連結)
		/// 此端點不需要登入, 但需要有效的 token
		/// </summary>
		// DELETE /api/members/confirm-delete
		[HttpDelete("confirm-delete")]
		[AllowAnonymous]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ConfirmDeleteAccount([FromBody] ConfirmDeleteAccountDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var result = await _memberService.ConfirmDeleteAccountAsync(dto);

			if (!result.IsSuccess)
			{
				return result.ErrorMessage switch
				{
					"invalid_or_expired_token" => BadRequest(new ErrorViewModel
					{
						Message = "無效或已過期的連結，請重新申請",
						ErrorCode = "invalid_or_expired_token"
					}),
					"invalid_token_type" => BadRequest(new ErrorViewModel
					{
						Message = "無效的 Token 類型",
						ErrorCode = "invalid_token_type"
					}),
					"member_not_found" => NotFound(new ErrorViewModel
					{
						Message = "找不到會員",
						ErrorCode = "member_not_found"
					}),
					"member_blacklisted" => BadRequest(new ErrorViewModel
					{
						Message = "此帳號已被停權",
						ErrorCode = "member_blacklisted"
					}),
					"password_required" => BadRequest(new ErrorViewModel
					{
						Message = "請輸入密碼以確認刪除",
						ErrorCode = "password_required"
					}),
					"wrong_password" => BadRequest(new ErrorViewModel
					{
						Message = "密碼不正確",
						ErrorCode = "wrong_password"
					}),
					_ => BadRequest(new ErrorViewModel { Message = "刪除失敗，請稍後再試" })
				};
			}

			return Ok(new SuccessViewModel
			{
				Message = "帳號已成功刪除，感謝您使用我們的服務"
			});
		}

		// GET /api/members/validate-delete-token?token=xxx
		[HttpGet("validate-delete-token")]
		[AllowAnonymous]
		[EnableRateLimiting("AuthPolicy")]
		public async Task<IActionResult> ValidateDeleteToken([FromQuery] string token)
		{
			if (string.IsNullOrWhiteSpace(token))
				return BadRequest(new ErrorViewModel
				{
					Message = "連結無效或已過期",
					ErrorCode = "invalid_or_expired_token"
				});

			var result = await _memberService.ValidateDeleteTokenAsync(token);

			if (!result.IsSuccess)
				return BadRequest(new ErrorViewModel
				{
					Message = "連結無效或已過期",
					ErrorCode = "invalid_or_expired_token"
				});

			return Ok(new SuccessViewModel { Message = "token 有效" });
		}
	}
}
