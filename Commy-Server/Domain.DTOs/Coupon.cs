namespace Domain.DTOs
{
    public class Coupon
    {
        public long Id { get; set; }
        public long Cid { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Type { get; set; } = string.Empty;
        public CouponExtraData Extra { get; set; } = null!;
        public string ImageURI { get; set; } = string.Empty;
    }
}
