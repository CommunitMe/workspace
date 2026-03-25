using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class EventByCommunityCriterion : ICriterion<Event>
	{
		public long CommunityId { get; set; }

		public object[] GetKeys()
		{
			return new object[] { };
		}

		public bool IsMet(Event entity)
		{
			return entity.RelevantCommunity == CommunityId;
		}
	}
}
