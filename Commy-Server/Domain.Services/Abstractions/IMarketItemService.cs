using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface IMarketItemService
	{
		Task<List<MarketItem>?> GetMarketItemByCommunityId(long cid, int? maxAmount = null);
		Task<MarketItem?> GetMarketItemByID(long eid);
		Task<MarketItem> CreateMarketItem(MarketItem model);
		Task<MarketItem> UpdateMarketItem(MarketItem newMarketItemData);
		Task DeleteMarketItem(MarketItem marketItemData);
	}
}
