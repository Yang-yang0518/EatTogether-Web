namespace EatTogether.Models.DTOs
{
    public class MemberOrderItemDto
    {
        public string ProductName { get; set; } = "";
        public int Qty { get; set; }
        public bool IsSetMeal { get; set; }
        public string? Note { get; set; }
        public List<MemberOrderSubItemDto> SubItems { get; set; } = new();
    }

    public class MemberOrderSubItemDto
    {
        public string ProductName { get; set; } = "";
        public int Qty { get; set; }
    }
}
