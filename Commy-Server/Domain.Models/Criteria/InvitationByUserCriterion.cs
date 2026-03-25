using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class InvitationByUserCriterion : ICriterion<Invitation>
    {
        public long UserId { get; set; }

        public object[] GetKeys()
        {
            return new object[] { };
        }

        public bool IsMet(Invitation entity)
        {
            return entity.CreatorId == UserId;
        }
    }
}