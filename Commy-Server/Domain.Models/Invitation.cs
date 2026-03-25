using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
    public class Invitation : IPersistantModel<Invitation>
    {
        public Guid Id { get; set; }

        public int InvitationType { get; set; }

        public DateTime CreationTime { get; set; }

        public long CommunityId { get; set; }

        public long CreatorId { get; set; }

        public DateTime? Expiry { get; set; }

        public int? AllowedUsages { get; set; }

        public ICriterion<Invitation> GetCriterion()
        {
            return new InvitationCriterion { Id = Id };
        }
    }
}