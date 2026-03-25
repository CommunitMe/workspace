using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class Coupon : IPersistantModel<Coupon>
	{
		public long Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public long RelevantCommunity { get; set; }
		public long ServiceProviderId { get; set; }
		public long Category { get; set; }
		public int? Amount { get; set; }
		public DateTime? Expiration { get; set; }
		public bool IsActive { get; set; }
		public string? LocationName { get; set; }
		public decimal Price { get; set; }
		public string ImageUid { get; set; } = null!;
		public List<TextTag> Tags { get; set; } = null!;

		public ICriterion<Coupon> GetCriterion()
		{
			return new CouponCriterion { Id = Id };
		}
	}
}
