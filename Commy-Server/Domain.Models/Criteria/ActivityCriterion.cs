using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class ActivityCriterion : ICriterion<Activity>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Activity entity)
		{
			return entity.Id == Id;
		}
	}
}
