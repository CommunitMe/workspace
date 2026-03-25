using Domain.Abstractions.Common;
using Domain.Models.ActivityDetails;
using Domain.Models.Criteria;
using Domain.Models.Enums;

namespace Domain.Models
{
	public class Activity : IPersistantModel<Activity>
	{
		public long Id { get; set; }
		public DateTime InsertTime { get; set; }
		public ActivityType Type { get; set; } = null!;
		public ActivityDetailsBase ActivityDetails { get; set; } = null!;

		public ICriterion<Activity> GetCriterion()
		{
			return new ActivityCriterion { Id = Id };
		}
	}
}