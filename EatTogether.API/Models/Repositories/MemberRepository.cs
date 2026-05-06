using EatTogether.API.Models.DTOs;
using EatTogether.API.Models.EfModels;
using EatTogether.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace EatTogether.API.Models.Repositories
{
	public interface IMemberRepository
	{
		Task<MemberDetailDto?> GetByIdAsync(int id);
		Task<MemberListDto?> GetByPhoneAsync(string phone);
		// -----內用點餐頁用------------------------------
		Task<Member?> GetByEmailAsync(string email);

		// -----前台會員註冊用------------------------------
		Task<bool> IsAccountExistsAsync(string account);
		Task<bool> IsEmailExistsAsync(string email);
		Task CreateMemberAsync(Member member);
		Task<bool> HasPendingConfirmTokenAsync(int memberId);
		Task CreateConfirmTokenAsync(MemberConfirmToken token);

		// -----前台一般登入用------------------------------
		Task<Member?> GetMemberByAccountAsync(string account);
		Task SaveRefreshTokenAsync(MemberRefreshToken token);

		// -----前台登出用------------------------------
		Task RevokeRefreshTokenByTokenStringAsync(string token);

		// -----前台 Refresh Token 用------------------------------
		Task<MemberRefreshToken?> GetRefreshTokenAsync(string token);
		Task RevokeRefreshTokenAsync(int tokenId);

		// -----前台復原帳號用------------------------------
		Task RestoreAccountAsync(int memberId);

		// -----前台忘記密碼用------------------------------
		Task<bool> HasPendingPasswordResetTokenAsync(int memberId);
		Task CreatePasswordResetTokenAsync(MemberPasswordResetToken token);

		// -----前台重設密碼用------------------------------
		Task<MemberPasswordResetToken?> GetPasswordResetTokenAsync(string token);
		Task MarkPasswordResetTokenUsedAsync(int tokenId);
		Task UpdateMemberPasswordAsync(int memberId, string hashedPassword);
		Task RevokeAllRefreshTokensByMemberIdAsync(int memberId);

		// -----前台 Email 驗證用------------------------------
		Task<MemberConfirmToken?> GetConfirmTokenAsync(string token);
		Task MarkConfirmTokenUsedAsync(int tokenId);
		Task SetMemberConfirmedAsync(int memberId);
		Task UpdateMemberEmailAsync(int memberId, string newEmail);
		Task ConfirmMemberAndMarkTokenUsedAsync(int memberId, int tokenId);
		Task UpdateEmailAndMarkTokenUsedAsync(int memberId, string newEmail, int tokenId);
		Task MarkOldConfirmTokensUsedAsync(int memberId);
		Task MarkOldEmailChangeTokensUsedAsync(int memberId);
		Task<Member?> GetMemberByIdAsync(int id);

		// -----前台第三方登入用------------------------------
		Task<MemberExternalLogin?> GetExternalLoginByProviderAsync(string provider, string providerUserId);
		Task<MemberExternalLogin?> GetExternalLoginByMemberAndProviderAsync(int memberId, string provider);
		Task<List<MemberExternalLogin>> GetAllExternalLoginsByMemberIdAsync(int memberId);
		Task CreateExternalLoginAsync(MemberExternalLogin login);

		// -----前台個人資料用------------------------------
		Task<bool> UpdateProfileAsync(int memberId, UpdateProfileDto dto);
		Task UpdateAvatarAsync(int memberId, string avatarFileName);
		Task UpdateAccountAsync(int memberId, string account, string hashedPassword);
		Task DeleteExternalLoginAsync(int memberId, string provider);
		Task SoftDeleteMemberAsync(int memberId);
		Task<bool> IsEmailExistsForOtherAsync(int memberId, string email);
		Task MarkOldDeleteAccountTokensUsedAsync(int memberId);
	}

	public class MemberRepository : IMemberRepository
	{
		private readonly EatTogetherDBContext _context;

		public MemberRepository(EatTogetherDBContext context)
		{
			_context = context;
		}

		// 取單筆詳情
		public async Task<MemberDetailDto?> GetByIdAsync(int id)
		{
			return await _context.Members
				.AsNoTracking()
				.Where(m => m.Id == id)
				.Select(m => new MemberDetailDto
				{
					Id = m.Id,
					Name = m.Name,
					Account = m.Account,
					Email = m.Email,
					Phone = m.Phone,
					BirthDate = m.BirthDate,
					CreatedAt = m.CreatedAt,
					IsConfirmed = m.IsConfirmed,
					IsBlacklisted = m.IsBlacklisted,
					IsDeleted = m.IsDeleted,
					DeletedAt = m.DeletedAt,
					BlacklistReason = m.BlacklistReason,
					AvatarFileName = m.AvatarFileName,
				})
				.FirstOrDefaultAsync();
		}

		// 以電話號碼查單筆會員
		public async Task<MemberListDto?> GetByPhoneAsync(string phone)
		{
			var trimmed = phone.Trim();
			return await _context.Members
				.AsNoTracking()
				.Where(m => m.Phone == trimmed && !m.IsDeleted)
				.Select(m => new MemberListDto
				{
					Id = m.Id,
					Name = m.Name,
					Account = m.Account,
					Email = m.Email,
					Phone = m.Phone,
					BirthDate = m.BirthDate,
					CreatedAt = m.CreatedAt,
					IsConfirmed = m.IsConfirmed,
					IsBlacklisted = m.IsBlacklisted,
					IsDeleted = m.IsDeleted,
					DeletedAt = m.DeletedAt,
					BlacklistReason = m.BlacklistReason,
				})
				.FirstOrDefaultAsync();
		}


		// -----前台點餐頁用-----
		public async Task<Member?> GetByEmailAsync(string email)
		{
			return await _context.Members
				.AsNoTracking()
				.FirstOrDefaultAsync(m => m.Email == email && !m.IsDeleted);
		}

		// -----前台會員註冊用------------------------------

		// 帳號唯一性檢查（含軟刪除會員，避免違反 Unique Index）
		public async Task<bool> IsAccountExistsAsync(string account)
		{
			return await _context.Members.AnyAsync(m => m.Account == account);
		}

		// Email 唯一性檢查（含軟刪除會員，避免違反 Unique Index）
		public async Task<bool> IsEmailExistsAsync(string email)
		{
			return await _context.Members.AnyAsync(m => m.Email == email);
		}

		public async Task CreateMemberAsync(Member member)
		{
			_context.Members.Add(member);
			await _context.SaveChangesAsync();
		}

		// 寄信保護：查詢是否已有未過期且未使用的註冊驗證 token
		public async Task<bool> HasPendingConfirmTokenAsync(int memberId)
		{
			return await _context.MemberConfirmTokens.AnyAsync(t =>
				t.MemberId == memberId &&
				!t.IsUsed &&
				t.ExpiresAt > DateTime.Now &&
				t.NewEmail == null);
		}

		public async Task CreateConfirmTokenAsync(MemberConfirmToken token)
		{
			_context.MemberConfirmTokens.Add(token);
			await _context.SaveChangesAsync();
		}

		// -----前台 Email 驗證用------------------------------

		public async Task<MemberConfirmToken?> GetConfirmTokenAsync(string token)
		{
			return await _context.MemberConfirmTokens
				.FirstOrDefaultAsync(t => t.Token == token);
		}

		public async Task MarkConfirmTokenUsedAsync(int tokenId)
		{
			var token = await _context.MemberConfirmTokens.FindAsync(tokenId);
			if (token == null) return;
			token.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task SetMemberConfirmedAsync(int memberId)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.IsConfirmed = true;
			await _context.SaveChangesAsync();
		}

		public async Task UpdateMemberEmailAsync(int memberId, string newEmail)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.Email = newEmail;
			await _context.SaveChangesAsync();
		}

		public async Task ConfirmMemberAndMarkTokenUsedAsync(int memberId, int tokenId)
		{
			var member = await _context.Members.FindAsync(memberId);
			var token = await _context.MemberConfirmTokens.FindAsync(tokenId);
			if (member == null || token == null) return;

			member.IsConfirmed = true;
			token.IsUsed = true;
			await _context.SaveChangesAsync(); // 兩個變更一次提交，EF Core 自動包隱式 Transaction
		}

		public async Task UpdateEmailAndMarkTokenUsedAsync(int memberId, string newEmail, int tokenId)
		{
			var member = await _context.Members.FindAsync(memberId);
			var token = await _context.MemberConfirmTokens.FindAsync(tokenId);
			if (member == null || token == null) return;

			member.Email = newEmail;
			token.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task MarkOldConfirmTokensUsedAsync(int memberId)
		{
			var tokens = await _context.MemberConfirmTokens
				.Where(t => t.MemberId == memberId && !t.IsUsed && t.NewEmail == null)
				.ToListAsync();
			if (tokens.Count == 0) return;
			foreach (var t in tokens)
				t.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		// 申請 Email 變更前呼叫，將同一會員舊的未使用 Email 變更 token 標為已使用，
		// 確保同一時間只有最新的連結有效
		public async Task MarkOldEmailChangeTokensUsedAsync(int memberId)
		{
			var tokens = await _context.MemberConfirmTokens
				.Where(t => t.MemberId == memberId && !t.IsUsed && t.NewEmail != null)
				.ToListAsync();
			if (tokens.Count == 0) return;
			foreach (var t in tokens)
				t.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task<Member?> GetMemberByIdAsync(int id)
		{
			return await _context.Members.FindAsync(id);
		}

		// -----前台一般登入用------------------------------

		public async Task<Member?> GetMemberByAccountAsync(string account)
		{
			return await _context.Members
				.Include(m => m.MemberExternalLogins)
				.FirstOrDefaultAsync(m => m.Account == account);
		}

		public async Task SaveRefreshTokenAsync(MemberRefreshToken token)
		{
			_context.MemberRefreshTokens.Add(token);
			await _context.SaveChangesAsync();
		}

		// -----前台登出用------------------------------

		public async Task RevokeRefreshTokenByTokenStringAsync(string token)
		{
			var refreshToken = await _context.MemberRefreshTokens
				.FirstOrDefaultAsync(t => t.Token == token);
			if (refreshToken == null || refreshToken.IsRevoked) return;
			refreshToken.IsRevoked = true;
			await _context.SaveChangesAsync();
		}

		// -----前台 Refresh Token 用------------------------------

		// 含 Member 及其 MemberExternalLogins，供 RefreshTokenAsync 建構 MemberViewModel
		public async Task<MemberRefreshToken?> GetRefreshTokenAsync(string token)
		{
			return await _context.MemberRefreshTokens
				.Include(t => t.Member)
				.ThenInclude(m => m.MemberExternalLogins)
				.FirstOrDefaultAsync(t => t.Token == token);
		}

		public async Task RevokeRefreshTokenAsync(int tokenId)
		{
			var token = await _context.MemberRefreshTokens.FindAsync(tokenId);
			if (token == null) return;
			token.IsRevoked = true;
			await _context.SaveChangesAsync();
		}

		// -----前台復原帳號用------------------------------

		public async Task RestoreAccountAsync(int memberId)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.IsDeleted = false;
			member.DeletedAt = null;
			await _context.SaveChangesAsync();
		}

		// -----前台忘記密碼用------------------------------

		// 寄信保護：查詢是否已有未過期且未使用的密碼重設 token
		public async Task<bool> HasPendingPasswordResetTokenAsync(int memberId)
		{
			return await _context.MemberPasswordResetTokens.AnyAsync(t =>
				t.MemberId == memberId &&
				!t.IsUsed &&
				t.ExpiresAt > DateTime.Now);
		}

		public async Task CreatePasswordResetTokenAsync(MemberPasswordResetToken token)
		{
			_context.MemberPasswordResetTokens.Add(token);
			await _context.SaveChangesAsync();
		}

		// -----前台重設密碼用------------------------------

		// 含 Member 導覽屬性，供驗證 IsDeleted / IsBlacklisted
		public async Task<MemberPasswordResetToken?> GetPasswordResetTokenAsync(string token)
		{
			return await _context.MemberPasswordResetTokens
				.Include(t => t.Member)
				.FirstOrDefaultAsync(t => t.Token == token);
		}

		public async Task MarkPasswordResetTokenUsedAsync(int tokenId)
		{
			var token = await _context.MemberPasswordResetTokens.FindAsync(tokenId);
			if (token == null) return;
			token.IsUsed = true;
			await _context.SaveChangesAsync();
		}

		public async Task UpdateMemberPasswordAsync(int memberId, string hashedPassword)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.HashedPassword = hashedPassword;
			await _context.SaveChangesAsync();
		}

		// 密碼重設後撤銷該會員所有 RefreshToken（踢除所有裝置）
		public async Task RevokeAllRefreshTokensByMemberIdAsync(int memberId)
		{
			var tokens = await _context.MemberRefreshTokens
				.Where(t => t.MemberId == memberId && !t.IsRevoked)
				.ToListAsync();
			if (tokens.Count == 0) return;
			foreach (var t in tokens)
				t.IsRevoked = true;
			await _context.SaveChangesAsync();
		}

		// -----前台第三方登入用------------------------------

		/// <summary>
		/// 以 Provider + ProviderUserId 查詢外部登入紀錄（含 Member 導覽屬性）
		/// 用於 OAuth 登入時檢查使用者是否已註冊
		/// </summary>
		public async Task<MemberExternalLogin?> GetExternalLoginByProviderAsync(string provider, string providerUserId)
		{
			return await _context.MemberExternalLogins
				.Include(e => e.Member)
				.FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderUserId == providerUserId);
		}

		/// <summary>
		/// 查詢會員是否已綁定指定 Provider
		/// 用於檢查單一第三方登入狀態（如：檢查是否已綁定 Google）
		/// </summary>
		public async Task<MemberExternalLogin?> GetExternalLoginByMemberAndProviderAsync(int memberId, string provider)
		{
			return await _context.MemberExternalLogins
				.FirstOrDefaultAsync(e => e.MemberId == memberId && e.Provider == provider);
		}

		/// <summary>
		/// 查詢會員的所有第三方登入紀錄
		/// 用於個人資料頁一次取得所有連結狀態
		/// </summary>
		public async Task<List<MemberExternalLogin>> GetAllExternalLoginsByMemberIdAsync(int memberId)
		{
			return await _context.MemberExternalLogins
				.Where(e => e.MemberId == memberId)
				.ToListAsync();
		}

		/// <summary>
		/// 新增外部登入紀錄
		/// </summary>
		public async Task CreateExternalLoginAsync(MemberExternalLogin login)
		{
			_context.MemberExternalLogins.Add(login);
			await _context.SaveChangesAsync();
		}

		// -----前台個人資料用------------------------------

		public async Task<bool> UpdateProfileAsync(int memberId, UpdateProfileDto dto)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return false;

			member.Name = dto.Name;
			member.Phone = dto.Phone;
			member.BirthDate = dto.BirthDate;
			await _context.SaveChangesAsync();
			return true;
		}

		public async Task UpdateAvatarAsync(int memberId, string avatarFileName)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.AvatarFileName = avatarFileName;
			await _context.SaveChangesAsync();
		}

		public async Task UpdateAccountAsync(int memberId, string account, string hashedPassword)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.Account = account;
			member.HashedPassword = hashedPassword;
			await _context.SaveChangesAsync();
		}

		/// <summary>
		/// 刪除指定會員的指定 Provider 外部登入紀錄（硬刪除）
		/// 用於取消連結單一第三方登入（如：取消連結 Google）
		/// </summary>
		public async Task DeleteExternalLoginAsync(int memberId, string provider)
		{
			int deletedCount = await _context.MemberExternalLogins
				.Where(e => e.MemberId == memberId && e.Provider == provider)
				.ExecuteDeleteAsync();
		}


		public async Task SoftDeleteMemberAsync(int memberId)
		{
			var member = await _context.Members.FindAsync(memberId);
			if (member == null) return;
			member.IsDeleted = true;
			member.DeletedAt = DateTime.Now;
			await _context.SaveChangesAsync();
		}

		public async Task<bool> IsEmailExistsForOtherAsync(int memberId, string email)
		{
			return await _context.Members
				.AnyAsync(m => m.Email == email && m.Id != memberId);
		}

		/// <summary>
		/// 申請刪除帳號前呼叫,將同一會員舊的未使用刪除帳號 token 標為已使用,
		/// 確保同一時間只有最新的連結有效
		/// </summary>
		public async Task MarkOldDeleteAccountTokensUsedAsync(int memberId)
		{
			var tokens = await _context.MemberConfirmTokens
				.Where(t => t.MemberId == memberId &&
							!t.IsUsed &&
							t.NewEmail == "DELETE_ACCOUNT")
				.ToListAsync();

			if (tokens.Count == 0) return;

			foreach (var t in tokens)
				t.IsUsed = true;

			await _context.SaveChangesAsync();
		}
	}
}
