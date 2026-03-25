using Domain.Abstractions.Common;

namespace Domain.Models.Abstractions.Repositories
{
	public interface IProfileRepository : IRepository<Profile>
	{
		Task<List<Profile>> GetProfilesByCommunity(ICriterion<Community> community);
	}
}
