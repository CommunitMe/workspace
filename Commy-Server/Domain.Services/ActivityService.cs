using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
	public class ActivityService : IActivityService
	{
		private readonly ILogger<ActivityService> logger;
		private readonly ICommunityService communityService;
		private readonly IActivityRespository activityRepository;

		public ActivityService(
			ILogger<ActivityService> logger,
			ICommunityService communityService,
			IActivityRespository activityRepository)
		{
			this.logger = logger;
			this.communityService = communityService;
			this.activityRepository = activityRepository;
		}

		public async Task<List<Activity>?> GetActivitiesBefore(DateTime startTime, int? maxAmount = null)
		{
			IEnumerable<Activity> activities = await activityRepository.GetAll(new ActivityBeforeCriterion { Before = startTime });

			if (maxAmount.HasValue)
			{
				activities = activities.Take(maxAmount.Value);
			}

			return activities.ToList();
		}
	}
}