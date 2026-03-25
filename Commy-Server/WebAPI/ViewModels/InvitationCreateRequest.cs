using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTo(typeof(Domain.Models.Invitation))]
    public class InvitationCreateRequest
    {
        public int InvitationType { get; set; }
        [AdaptMember(nameof(Domain.Models.Invitation.CommunityId))]
        public string CommunityId { get; set; } = string.Empty;
        public int? AllowedUsages { get; set; }
        public int ExpirationDays { get; set; }
    }
}