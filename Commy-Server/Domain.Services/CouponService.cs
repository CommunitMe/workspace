using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Models.Enums;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
	public class CouponService : ICouponService, ISearchProvider
	{
		private readonly ILogger<CouponService> logger;
		private readonly IDatabaseTransaction db;

		public CouponService(ILogger<CouponService> logger, IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task<Coupon?> GetCouponById(long id)
		{
			var coupon = await db.Coupons.GetById(new CouponCriterion { Id = id });

			if (coupon == null)
			{
				this.logger.LogInformation(message: "No coupon item with id '{0}'", id);
			}

			return coupon;
		}

		public async Task<List<Coupon>?> GetCouponsByCommunityId(long cid, int? maxAmount = null)
		{
			var filter = new CouponByCommunityCriterion { CommunityId = cid };

			IEnumerable<Coupon> result = await db.Coupons.GetAll(filter);

			if (maxAmount.HasValue)
			{
				result = result.Take(maxAmount.Value);
			}

			return result.ToList();
		}

		public async Task<Coupon> CreateCoupon(Coupon couponData)
		{
			couponData = await db.Coupons.Create(couponData);
			await db.SaveChangesAsync();
			return couponData;
		}

		public async Task<Coupon> UpdateCoupon(Coupon couponData)
		{
			couponData = await db.Coupons.Update(couponData);
			await db.SaveChangesAsync();
			return couponData;
		}

		public async Task DeleteCoupon(Coupon couponData) 
		{
			await db.Coupons.Delete(couponData);
			await db.SaveChangesAsync();
		}

        public async Task<SearchResult?> GetSearchResults(string searchTerm, long communityId, string? scKeys)
        {
            var coupons = (await GetCouponsByCommunityId(communityId))?
                .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()))
                .ToList() ?? new List<Coupon>();

            if (coupons.Count == 0)
            {
                return null;
            }

            return new SearchResult
            {
                EntityTypeId = EntityType.Coupons.ID,
                EntityIds = coupons.Select(m => m.Id).ToList(),
            };
        }

        public EntityType GetSearchResultType()
        {
            return EntityType.Coupons;
        }
    }
}
