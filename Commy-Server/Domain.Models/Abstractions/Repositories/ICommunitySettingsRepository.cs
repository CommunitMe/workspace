using Domain.Abstractions.Common;

namespace Domain.Models.Abstractions.Repositories
{
    public interface ICommunitySettingsRepository : IRepository<CommunitySettings>
    {
        Task<CommunitySettings?> GetByCommunityId(long id);
    }
}