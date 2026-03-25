using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface IInvitationService
    {
        Task<List<Invitation>?> GetInvitationsByCommunityId(long cid, int? maxAmount = null);
        Task<List<Invitation>?> GetInvitationsByUserId(long uid, int? maxAmount = null);
        Task<Invitation?> GetInvitationById(Guid id);
        Task<Invitation> CreateInvitation(Invitation invitationData);
        Task<Invitation> UpdateInvitation(Invitation invitationData);
        Task DeleteInvitation(Invitation invitationData);
    }
}