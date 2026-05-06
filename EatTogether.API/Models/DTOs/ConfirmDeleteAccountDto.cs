using System.ComponentModel.DataAnnotations;

namespace EatTogether.API.Models.DTOs
{
	/// <summary>
	/// 確認刪除帳號請求 DTO (第二步,點擊信中連結)
	/// </summary>
	public class ConfirmDeleteAccountDto
	{
		/// <summary>
		/// 刪除確認 token
		/// </summary>
		[Required(ErrorMessage = "token_required")]
		[StringLength(32, MinimumLength = 32, ErrorMessage = "token_invalid")]
		public string Token { get; set; } = string.Empty;

		/// <summary>
		/// 一般帳號在確認時仍需密碼 (純第三方帳號不需要)
		/// </summary>
		public string? Password { get; set; }
	}
}
