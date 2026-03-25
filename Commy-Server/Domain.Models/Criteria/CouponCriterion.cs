using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class CouponCriterion : ICriterion<Coupon>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Coupon entity)
		{
			return entity.Id == Id;
		}
	}
}
