using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
	public class NotificationService : INotificationService
	{
		private readonly ILogger<NotificationService> logger;
		private readonly IDatabaseTransaction db;

		public NotificationService(
			ILogger<NotificationService> logger,
			IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task<Notification> CreateNotification(Notification eventData)
		{
			eventData = await db.Notifications.Create(eventData);
			await db.SaveChangesAsync();
			return eventData;
		}

		public async Task<List<Notification>?> GetProfileNotificationsAsync(long pid)
		{
			var profile = await db.Profiles.GetById(new ProfileCriterion { Id = pid });

			if (profile == null)
			{
				logger.LogInformation("No profile with id '{0}', cannot get profile notifications", pid);
				return null;
			}

			var profileCommunities = await db.Communities.GetCommunitiesByProfile(profile.GetCriterion());
			var profileCommunityIds = profileCommunities.Select(community => community.Id);
			return await db.Notifications.GetAll(new NotificationByCommunitiesCriterion { CommunityIds = profileCommunityIds });
		}
	}
}
