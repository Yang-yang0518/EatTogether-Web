namespace EatTogether.Models.DTOs
{
    public class UpdatePickupInfoDto
    {
        public string OrderNumber   { get; set; } = "";
        public string PickupTime    { get; set; } = "";
        public string CustomerName  { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public bool   Utensils      { get; set; }
    }
}
