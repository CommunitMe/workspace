using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class CommunityProfileCriterion : ICriterion<CommunityProfile>
    {
        public long CommunityId { get; set; }

        public long ProfileId { get; set; }

        public object[] GetKeys()
        {
            return new object[]
            {
                CommunityId,
                ProfileId
            };
        }

        public bool IsMet(CommunityProfile entity)
        {
            return entity.CommunityId == CommunityId && entity.ProfileId == ProfileId;
        }
    }
}
