using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class EventCriterion : ICriterion<Event>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Event entity)
		{
			return entity.Id == Id;
		}
	}
}
