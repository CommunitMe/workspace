using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface ICommunityProfileService
    {
        Task UpdateMemberState(CommunityProfile communityProfile);
        Task UpdateProviderState(CommunityProfile communityProfile);
    }
}