using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class NotificationByCommunitiesCriterion : ICriterion<Notification>
	{
		public IEnumerable<long> CommunityIds { get; set; } = null!;

		public object[] GetKeys()
		{
			return new object[] { };
		}

		public bool IsMet(Notification entity)
		{
			return CommunityIds.Contains(entity.RelevantCommunity);
		}
	}
}
