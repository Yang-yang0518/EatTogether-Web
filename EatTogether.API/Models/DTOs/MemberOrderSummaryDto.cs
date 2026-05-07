namespace EatTogether.Models.DTOs
{
    public class MemberOrderSummaryDto
    {
        public string OrderNumber { get; set; } = "";
        public DateTime OrderAt { get; set; }
        public int TotalAmount { get; set; }
		public int OriginalAmount { get; set; }
		public int DiscountAmount { get; set; }
		public string? CouponCode { get; set; }
		public string? EventTitle { get; set; }
		public string PayMethod { get; set; } = "";
        public bool InOrOut { get; set; }   // true=內用 / false=外帶
		public List<MemberOrderItemDto> Items { get; set; } = new();
	}
}
