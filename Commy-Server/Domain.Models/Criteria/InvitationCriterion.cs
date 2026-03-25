using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class InvitationCriterion : ICriterion<Invitation>
    {
        public Guid Id { get; set; }

        public object[] GetKeys()
        {
            return new object[]
            {
                Id
            };
        }

        public bool IsMet(Invitation entity)
        {
            return entity.Id == Id;
        }
    }
}