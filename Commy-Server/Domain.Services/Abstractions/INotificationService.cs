using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface INotificationService
	{
		Task<Notification> CreateNotification(Notification model);
		Task<List<Notification>?> GetProfileNotificationsAsync(long pid);
	}
}
