using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class MarketItem : IPersistantModel<MarketItem>
	{
		public long Id { get; set; }

		public long Owner { get; set; }

		public long Community { get; set; }

		public long Category { get; set; }

		public string Description { get; set; } = null!;

		public decimal Price { get; set; }

		public short PriceCurrency { get; set; }
		public List<TextTag> Tags { get; set; } = null!;
		public string ImageUid { get; set; } = null!;

		public ICriterion<MarketItem> GetCriterion()
		{
			return new MarketItemCriterion { Id = Id };
		}
	}
}
