using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface ICommunitySettingsService
    {
        Task<CommunitySettings> CreateCommunitySettings(CommunitySettings settings);
        Task<CommunitySettings?> GetCommunitySettingsByCommunityId(long cid);
        Task UpdateCommunitySettings(CommunitySettings settings);
    }
}