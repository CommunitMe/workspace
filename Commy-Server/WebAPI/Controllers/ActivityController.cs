using Domain.Models.ActivityDetails;
using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/activity")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class ActivityController : Controller
    {
        private readonly ILogger<ActivityController> logger;
        private readonly IActivityService activityService;
        private readonly IProfilesService profilesService;
        private readonly LinkGenerator linkGenerator;

        public ActivityController(
            ILogger<ActivityController> logger,
            IActivityService activityService,
            IProfilesService profilesService,
            LinkGenerator linkGenerator)
        {
            this.logger = logger;
            this.activityService = activityService;
            this.profilesService = profilesService;
            this.linkGenerator = linkGenerator;
        }

        [HttpGet("fetch")]
        public async Task<IActionResult> FetchActivities(long? startTime, int? max)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            if (startTime.HasValue == false)
            {
                logger.LogError("Missing input parameter 'startTime'");
                return UnprocessableEntity("Invalid parameters");
            }

            if (max <= 0 || max > 10)
            {
                logger.LogError("Input parameter 'max' is out of range");
                return UnprocessableEntity("Invalid parameters");
            }

            DateTime dtStarTime = DateTimeOffset.FromUnixTimeMilliseconds(startTime.Value).DateTime;
            var data = await activityService.GetActivitiesBefore(dtStarTime, max);

            if (data == null)
            {
                logger.LogError("Failed to get activites");
                return UnprocessableEntity();
            }

            IEnumerable<ViewModels.Activity?> activities = data.Select(model =>
            {
                var activityParams = GetActivityParams(model);

                if (activityParams == null)
                {
                    logger.LogError("Failed to parse activity params for activity id '{0}' of type '{1}'", model.Id, model.Type.Name);
                    return null;
                }

                string imageUri = $"{Request.Scheme}://{Request.Host}/communitme/api/profiles/profile/{activityParams.Pid}/image";

                return new ViewModels.Activity
                {
                    Timestamp = new DateTimeOffset(model.InsertTime).ToUnixTimeMilliseconds(),
                    ActivityType = model.Type.Name,
                    ActivityParams = activityParams,
                    ImageURI = imageUri
                };
            });

            return Ok(activities);
        }

        private ActivityParams? GetActivityParams(Domain.Models.Activity activity)
        {
            if (activity == null)
            {
                return null;
            }

            ActivityParams result = new ActivityParams();

            switch (activity.ActivityDetails)
            {
                case LookingForServiceActivityDetails details:
                    result.Pid = details.ProfileId.ToString();
                    result.ServiceType = details.ServiceType.Name;
                    result.ServiceDuration = details.ServiceDuration.Name;
                    break;
                case NewServiceProviderActivityDetails details:
                    result.Pid = details.ProfileId.ToString();
                    result.ServiceType = details.ServiceType.Name;
                    result.CommunityId = details.CommunityId.ToString();
                    break;
                case SellingActivityDetails details:
                    result.Pid = details.ProfileId.ToString();
                    result.MarketItemId = details.MarketItemId.ToString();
                    break;
                default:
                    logger.LogError("Missing conversion method for Activity Details of type '{0}'", activity.ActivityDetails.GetType().Name);
                    return null;
            }

            return result;
        }
    }
}
