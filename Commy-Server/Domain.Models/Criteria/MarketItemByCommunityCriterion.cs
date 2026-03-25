using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class MarketItemByCommunityCriterion : ICriterion<MarketItem>
	{
		public long CommunityId { get; set; }

		public object[] GetKeys()
		{
			return new object[] { };
		}

		public bool IsMet(MarketItem entity)
		{
			return entity.Community == CommunityId;
		}
	}
}
