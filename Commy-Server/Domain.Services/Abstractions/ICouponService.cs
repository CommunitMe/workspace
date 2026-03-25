using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface ICouponService
	{
		Task<List<Coupon>?> GetCouponsByCommunityId(long cid, int? maxAmount = null);
		Task<Coupon?> GetCouponById(long id);
		Task<Coupon> CreateCoupon(Coupon couponData);
		Task<Coupon> UpdateCoupon(Coupon couponData);
		Task DeleteCoupon(Coupon couponData);
	}
}
