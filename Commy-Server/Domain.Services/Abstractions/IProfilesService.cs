using Domain.Models;
using System.Security.Claims;

namespace Domain.Services.Abstractions
{
    public interface IProfilesService
    {
        Task<Profile?> GetProfileDataAsync(long id);
        Task<Profile?> GetUserProfile(ClaimsPrincipal user);
        Task<List<Profile>> GetProfilesByCommunityId(long cid);
        Task SetLastOpenedNotifications(long uid, DateTime lastOpenTime);
        Task SetLastLogin(long uid, DateTime loginTime);
    }
}
