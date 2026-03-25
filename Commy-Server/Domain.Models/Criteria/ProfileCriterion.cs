using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class ProfileCriterion : ICriterion<Profile>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Profile entity)
		{
			return entity.Id == Id;
		}
	}
}
