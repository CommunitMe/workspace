using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class CouponByCommunityCriterion : ICriterion<Coupon>
	{
		public long CommunityId { get; set; }

		public object[] GetKeys()
		{
			return new object[] { };
		}

		public bool IsMet(Coupon entity)
		{
			return entity.RelevantCommunity == CommunityId;
		}
	}
}
