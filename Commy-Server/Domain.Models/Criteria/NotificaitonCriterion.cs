using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class NotificationCriterion : ICriterion<Notification>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Notification entity)
		{
			return entity.Id == Id;
		}
	}
}
