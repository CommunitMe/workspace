using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
	public class CommunityService : ICommunityService
	{
		private readonly ILogger<CommunityService> logger;
		private readonly IDatabaseTransaction db;

		public CommunityService(ILogger<CommunityService> logger, IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task<Community> CreateCommunity(Profile creator, Community communityData)
		{
			communityData = await db.Communities.Create(communityData);
			await AssignProfileToCommunity(creator, communityData);

			return communityData;
		}

		public async Task<Community?> GetCommunityById(long cid)
		{
			var community = await db.Communities.GetById(new CommunityCriterion { Id = cid });

			if (community == null)
			{
				logger.LogInformation("No community with id '{0}'", cid);
			}

			return community;

		}
		public async Task<List<Community>> GetCommunitiesByProfileId(long uid)
		{
			var criterion = new ProfileCriterion { Id = uid };
			return await db.Communities.GetCommunitiesByProfile(criterion);
		}

		public async Task<bool> IsCommunityVisibleToProfile(Profile profile, Community community)
		{
			var profileCommunities = await db.Communities.GetCommunitiesByProfile(profile.GetCriterion());

			return profileCommunities.Any(profileCommunity => profileCommunity == community);
		}

		public async Task UpdateCommunity(Community community)
		{
			await db.Communities.Update(community);
		}

		public async Task AssignProfileToCommunity(Profile profile, Community community)
		{
			var profileCommunities = await db.Communities.GetCommunitiesByProfile(profile.GetCriterion());

			if (profileCommunities.Any(profileCommunity => profileCommunity == community))
			{
				logger.LogInformation("Community '{0}' is already assigned to profile '{1}'", community.Id, profile.Id);
				return;
			}

			await db.Communities.AssignProfileToCommunity(profile.GetCriterion(), community.GetCriterion());

			await db.SaveChangesAsync();
		}
	}
}
