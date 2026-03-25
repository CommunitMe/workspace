using Domain.Abstractions.Common;

namespace Domain.Models.Abstractions.Repositories
{
	public interface ICommunityRepository : IRepository<Community>
	{
		Task AssignProfileToCommunity(ICriterion<Profile> criterion1, ICriterion<Community> criterion2);
		Task<List<Community>> GetCommunitiesByProfile(ICriterion<Profile> criterion);
		Task<bool> RemoveProfileFromCommunity(ICriterion<Profile> profileCriterion, ICriterion<Community> communityCriterion);
	}
}
