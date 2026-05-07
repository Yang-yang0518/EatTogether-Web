namespace EatTogether.API.Models.DTOs
{
	/// <summary>
	/// 申請刪除帳號請求 DTO (第一步)
	/// </summary>
	public class DeleteAccountDto
	{
		/// <summary>
		/// 一般帳號的密碼 (有密碼的帳號必填)
		/// </summary>
		public string? Password { get; set; }
	}
}
