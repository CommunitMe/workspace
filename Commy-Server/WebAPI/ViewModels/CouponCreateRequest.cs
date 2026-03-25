using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTo(typeof(Domain.Models.Coupon))]
    public class CouponCreateRequest
    {
        public string Id { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.Coupon.RelevantCommunity))]
        public string Cid { get; set; } = string.Empty;
        public string ServiceProviderId { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public CouponExtraData Extra { get; set; } = null!;
        public string ImageURI { get; set; } = string.Empty;
        [AdaptIgnore]
        public string[] Tags { get; set; } = null!;
    }
}
