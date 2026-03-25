using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface ICommunityService
	{
		Task AssignProfileToCommunity(Profile userProfile, Community newCommunity);
		Task<Community> CreateCommunity(Profile creator, Community community);
		Task<List<Community>> GetCommunitiesByProfileId(long uid);
		Task<Community?> GetCommunityById(long cid);
		Task<bool> IsCommunityVisibleToProfile(Profile profile, Community community);
		Task UpdateCommunity(Community community);
	}
}
