namespace EatTogether.Models.DTOs
{
    /// <summary>前台訂單查詢回傳結果</summary>
    public class OrderLookupResultDto
    {
        public string OrderNumber   { get; set; } = "";
        public string CustomerName  { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public string PickupTime    { get; set; } = "";
        /// <summary>0=待處理 1=已結帳</summary>
        public int    OrderStatus   { get; set; }
        public int    Subtotal      { get; set; }   // 餐點原價合計（折扣前）
        public int    DiscountAmount{ get; set; }   // 折扣總額（活動 + 優惠券）
        public int    TotalAmount   { get; set; }   // 實付金額
        public string Note          { get; set; } = "";   // 整筆備註

        // 活動
        public string? EventTitle        { get; set; }
        public string? EventDiscountType { get; set; }   // "FixedAmount" | "Percent" | "Gift"
        public int     EventDiscount     { get; set; }   // 活動折抵金額（Gift 為 0）

        // 優惠券
        public string? CouponCode     { get; set; }
        public int     CouponDiscount { get; set; }

        public List<OrderLookupItemDto> Items { get; set; } = new();
    }

    public class OrderLookupItemDto
    {
        public string  ProductName { get; set; } = "";
        public int     Qty         { get; set; }
        public int     UnitPrice   { get; set; }
        public bool    IsSetMeal   { get; set; }
        public bool    IsGift      { get; set; }   // 贈品項目（UnitPrice == 0）
        public string? ItemNote    { get; set; }
        /// <summary>子項目名稱清單（套餐用）</summary>
        public List<string> SubItems { get; set; } = new();
    }
}
