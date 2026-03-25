using Domain.Services;
using Domain.Services.Abstractions;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/notifications")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class NotificationController : Controller
    {
        private readonly ILogger<NotificationController> logger;
        private readonly IProfilesService profilesService;
        private readonly INotificationService notificationService;

        public NotificationController(
            ILogger<NotificationController> logger,
            IProfilesService profilesService,
            INotificationService notificationService
            )
        {
            this.logger = logger;
            this.profilesService = profilesService;
            this.notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var notifications = await notificationService.GetProfileNotificationsAsync(userProfile.Id);

            if (notifications == null)
            {
                logger.LogError("Failed to get notificaitons");
                return UnprocessableEntity();
            }

            var result = notifications.Select(m => m.Adapt<ViewModels.Notification>()).OrderByDescending(m => m.DateInserted);

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNotification([FromBody] WebAPI.ViewModels.NotificationCreateRequest notificationData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var model = notificationData.Adapt<Domain.Models.Notification>();
            model.InsertTime = DateTime.UtcNow;
            model = await notificationService.CreateNotification(model);

            if (model == null)
            {
                logger.LogError("Failed to create Notification");
                return UnprocessableEntity();
            }

            logger.LogInformation("New Notification created with id '{0}'", model.Id);

            return Ok(model.Id);
        }
    }
}
