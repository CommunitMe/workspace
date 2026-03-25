using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class InvitationByCommunityCriterion : ICriterion<Invitation>
    {
        public long CommunityId { get; set; }

        public object[] GetKeys()
        {
            return new object[] { };
        }

        public bool IsMet(Invitation entity)
        {
            return entity.CommunityId == CommunityId;
        }
    }
}