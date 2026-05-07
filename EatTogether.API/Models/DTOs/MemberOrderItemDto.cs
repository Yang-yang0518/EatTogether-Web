namespace EatTogether.Models.DTOs
{
    public class MemberOrderItemDto
    {
        public string ProductName { get; set; } = "";
        public int Qty { get; set; }
        public bool IsSetMeal { get; set; }
        public string? Note { get; set; }

		// ── 前台會員中心訂單紀錄頁專用 ────────────────
		public int? UnitPrice { get; set; }
	}
}
