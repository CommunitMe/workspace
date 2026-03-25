using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class CommunityCriterion : ICriterion<Community>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Community entity)
		{
			return entity.Id == Id;
		}
	}
}
