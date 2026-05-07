namespace EatTogether.Models.DTOs
{
    public class MemberOrderPagedResultDto
    {
        public List<MemberOrderSummaryDto> Items { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }
}
