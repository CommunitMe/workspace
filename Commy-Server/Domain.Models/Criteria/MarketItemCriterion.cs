using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class MarketItemCriterion : ICriterion<MarketItem>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(MarketItem entity)
		{
			return entity.Id == Id;
		}
	}
}
