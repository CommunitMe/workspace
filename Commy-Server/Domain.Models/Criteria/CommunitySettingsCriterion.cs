using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class CommunitySettingsCriterion : ICriterion<CommunitySettings>
    {
        public long Id { get; set; }

        public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(CommunitySettings entity)
		{
			return entity.Id == Id;
		}
    }
}