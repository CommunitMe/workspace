using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Models.Enums;
using Domain.Services.Abstractions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
	public class MarketItemService : IMarketItemService, ISearchProvider
	{
		private readonly ILogger<MarketItemService> logger;
		private readonly IDatabaseTransaction db;

		public MarketItemService(ILogger<MarketItemService> logger, IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task<MarketItem?> GetMarketItemByID(long eid)
		{
			var marketItem = await db.MarketItems.GetById(new MarketItemCriterion { Id = eid });

			if (marketItem == null)
			{
				this.logger.LogInformation(message: "No market item with id '{0}'", eid);
			}

			return marketItem;
		}

		public async Task<List<MarketItem>?> GetMarketItemByCommunityId(long cid, int? maxAmount = null)
		{
			var filter = new MarketItemByCommunityCriterion { CommunityId = cid };
			IEnumerable<MarketItem> result = await db.MarketItems.GetAll(filter);

			if (maxAmount.HasValue)
			{
				result = result.Take(maxAmount.Value);
			}

			return result.ToList();
		}

		public async Task<MarketItem> CreateMarketItem(MarketItem marketItemData)
		{
            marketItemData = await db.MarketItems.Create(marketItemData);
			await db.SaveChangesAsync();
			return marketItemData;
		}

		public async Task<MarketItem> UpdateMarketItem(MarketItem marketItemData)
		{
			marketItemData = await db.MarketItems.Update(marketItemData);
			await db.SaveChangesAsync();
			return marketItemData;
		}

		public async Task DeleteMarketItem(MarketItem marketItemData)
		{
			await db.MarketItems.Delete(marketItemData);
			await db.SaveChangesAsync();
		}

        public async Task<SearchResult?> GetSearchResults(string searchTerm, long communityId, string? scKeys)
        {
            var MarketItems = (await GetMarketItemByCommunityId(communityId))?
                .Where(p => p.Description.ToLower().Contains(searchTerm.ToLower()))
                .ToList() ?? new List<MarketItem>();

            if (MarketItems.Count == 0)
            {
                return null;
            }

            return new SearchResult
            {
                EntityTypeId = EntityType.MarketItems.ID,
                EntityIds = MarketItems.Select(m => m.Id).ToList(),
            };
        }

        public EntityType GetSearchResultType()
        {
            return EntityType.MarketItems;
        }
    }
}
