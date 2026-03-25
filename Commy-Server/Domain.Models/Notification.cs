using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class Notification : IPersistantModel<Notification>
	{
		public long Id { get; set; }
		public DateTime InsertTime { get; set; }
		public long RelevantCommunity { get; set; }
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;

		public ICriterion<Notification> GetCriterion()
		{
			return new NotificationCriterion { Id = Id };
		}
	}
}
