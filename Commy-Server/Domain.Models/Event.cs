using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class Event : IPersistantModel<Event>
	{
		public long Id { get; set; }
		public long RelevantCommunity { get; set; }
		public string Name { get; set; } = null!;
		public string Location { get; set; } = null!;
		public DateTime EventTime { get; set; }
		public string ImageUid { get; set; } = null!;
		public List<long> Interested { get; set; }

		public ICriterion<Event> GetCriterion()
		{
			return new EventCriterion { Id = Id };
		}
	}
}
