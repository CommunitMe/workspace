using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface IActivityService
	{
		Task<List<Activity>?> GetActivitiesBefore(DateTime startTime, int? maxAmount = null);
	}
}
