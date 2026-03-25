using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class CommunityProfileService : ICommunityProfileService
    {
        private readonly ILogger<CommunityProfileService> logger;
		private readonly IDatabaseTransaction db;
        
        public CommunityProfileService(ILogger<CommunityProfileService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task UpdateMemberState(CommunityProfile communityProfile)
        {
            var entity = await db.CommunityProfiles.GetById(communityProfile.GetCriterion());
            if (entity == null)
            {
                logger.LogInformation("No community profile with community id '{0}' ans profile id '{1}'", communityProfile.CommunityId, communityProfile.ProfileId);
                return;
            }

            communityProfile.ProviderState = entity.ProviderState;
            await db.CommunityProfiles.Update(communityProfile);
        }

        public async Task UpdateProviderState(CommunityProfile communityProfile)
        {
            var entity = await db.CommunityProfiles.GetById(communityProfile.GetCriterion());
            if (entity == null)
            {
                logger.LogInformation("No community profile with community id '{0}' ans profile id '{1}'", communityProfile.CommunityId, communityProfile.ProfileId);
                return;
            }

            communityProfile.MemberState = entity.MemberState;
            await db.CommunityProfiles.Update(communityProfile);
        }
    }
}