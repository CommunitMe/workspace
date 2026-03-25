using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class CommunitySettingsService : ICommunitySettingsService
    {
        private readonly ILogger<CommunitySettingsService> logger;
		private readonly IDatabaseTransaction db;
        
        public CommunitySettingsService(ILogger<CommunitySettingsService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<CommunitySettings> CreateCommunitySettings(CommunitySettings settings)
        {
            var communitySettings = await db.CommunitySettings.Create(settings);
            return communitySettings;
        }

        public async Task<CommunitySettings?> GetCommunitySettingsByCommunityId(long cid)
        {
            var communitySettings = await db.CommunitySettings.GetByCommunityId(cid);
            return communitySettings;
        }

        public async Task UpdateCommunitySettings(CommunitySettings settings)
        {
            await db.CommunitySettings.Update(settings);
        }
    }
}