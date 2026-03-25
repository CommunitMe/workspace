using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class ActivityBeforeCriterion : ICriterion<Activity>
	{
		public DateTime Before { get; set; }

		public object[] GetKeys()
		{
			return new object[] { };
		}

		public bool IsMet(Activity entity)
		{
			return entity.InsertTime < Before;
		}
	}
}
