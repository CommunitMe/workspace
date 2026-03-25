using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Models.Enums;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;
using Persistence.Database.Repositories;
using System.Security.Claims;

namespace Domain.Services
{
    public class ProfilesService : IProfilesService, ISearchProvider
    {
        private readonly ILogger<ProfilesService> logger;
        private readonly IDatabaseTransaction db;

        public ProfilesService(ILogger<ProfilesService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<Profile?> GetUserProfile(ClaimsPrincipal user)
        {
            Claim? userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);

            if (userIdClaim == null)
            {
                logger.LogError("Failed to get user identifier claim");
                return null;
            }

            if (!long.TryParse(userIdClaim.Value, out long userId))
            {
                logger.LogError("Failed parse user identifier claim");
                return null;
            }

            Profile? userProfile = await GetProfileDataAsync(userId);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for user id '{0}'", userId);
                return null;
            }

            return userProfile;
        }

        public async Task<Profile?> GetProfileDataAsync(long uid)
        {
            var profile = await db.Profiles.GetById(new ProfileCriterion { Id = uid });

            if (profile == null)
            {
                logger.LogInformation("No profile with id '{0}'", uid);
            }

            return profile;
        }

        public async Task<List<Profile>> GetProfilesByCommunityId(long cid)
        {
            var criterion = new CommunityCriterion { Id = cid };
            return await db.Profiles.GetProfilesByCommunity(criterion);
        }

        public async Task SetLastOpenedNotifications(long uid, DateTime lastOpenTime)
        {
            var profile = await db.Profiles.GetById(new ProfileCriterion { Id = uid });

            if (profile == null)
            {
                logger.LogError("Failed to get user profile for user id '{0}'", uid);
                return;
            }

            profile.LastOpenNotifications = lastOpenTime;
            await db.Profiles.Update(profile);
            await db.SaveChangesAsync();
        }

        public async Task SetLastLogin(long uid, DateTime loginTime)
        {
            var profile = await db.Profiles.GetById(new ProfileCriterion { Id = uid });

            if (profile == null)
            {
                logger.LogError("Failed to get user profile for user id '{0}'", uid);
                return;
            }

            profile.LastLogin = loginTime;
            await db.Profiles.Update(profile);
            await db.SaveChangesAsync();
        }

        public async Task<SearchResult?> GetSearchResults(string searchTerm, long communityId, string? scKeys)
        {
            var lowered = searchTerm.ToLower();
            var members = (await GetProfilesByCommunityId(communityId))
                .Where(p => (p.GivenName.ToLower() + " " + p.FamilyName.ToLower()).Contains(lowered))
                .ToList();
            if (members.Count == 0)
            {
                return null;
            }
            return new SearchResult
            {
                EntityTypeId = EntityType.Members.ID,
                EntityIds = members.Select(m => m.Id).ToList()
            };
        }

        public EntityType GetSearchResultType()
        {
            return EntityType.Members;
        }
    }
}
