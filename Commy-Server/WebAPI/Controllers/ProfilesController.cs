using Domain.Services.Abstractions;
using Domain.Services.Models;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/profiles")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class ProfilesController : Controller
    {
        private readonly ILogger<ProfilesController> logger;
        private readonly IProfilesService profileService;
        private readonly ICommunityService communityService;
        private readonly IImageService imageService;

        public ProfilesController(
            ILogger<ProfilesController> logger,
            IProfilesService profileService,
            ICommunityService communityService,
            IImageService imageService)
        {
            this.logger = logger;
            this.profileService = profileService;
            this.communityService = communityService;
            this.imageService = imageService;
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetIDs(long cid)
        {
            var userProfile = await profileService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var community = await communityService.GetCommunityById(cid);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", cid);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view events for community '{1}'", userProfile.Id, community.Id);
                return Unauthorized();
            }

            var communityProfiles = await profileService.GetProfilesByCommunityId(cid);

            if (communityProfiles == null)
            {
                logger.LogError("Failed to get events for community with id '{0}'", cid);
                return UnprocessableEntity();
            }

            var result = communityProfiles.Select(i => i.Id.ToString());
            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile(long uid)
        {
            if (uid == 0)
            {
                logger.LogError("Recieved id 0");
                return UnprocessableEntity();
            }

            var data = await profileService.GetProfileDataAsync(uid);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", uid);
                return UnprocessableEntity();
            }

            var response = data.Adapt<ProfileData>();
            response.ImageURI = $"{Request.Scheme}://{Request.Host}/communitme/api/profiles/profile/{data.Id}/image";

            return Ok(response);
        }

        [HttpGet("profile/{uid}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetProfileImage(long uid, [FromQuery] ProfileCircleProperties circleProperties)
        {
            if (uid == 0)
            {
                logger.LogError("Recieved id 0");
                return UnprocessableEntity();
            }

            var data = await profileService.GetProfileDataAsync(uid);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", uid);
                return UnprocessableEntity();
            }

            Stream? imageStream = data.ImageUid != null ? 
                imageService.GetImageByUID(Domain.Models.Enums.ImageLocationType.Profile, data.ImageUid, circleProperties.width, circleProperties.height) :
                imageService.GenerateCircle(data.GivenName, data.FamilyName, circleProperties);

            if (imageStream == null)
            {
                logger.LogError("Failed to get image for id {0}", uid);
                return UnprocessableEntity();
            }

            var result = new FileStreamResult(imageStream, new MediaTypeHeaderValue("image/jpeg"));
            return result;
        }

        [HttpPost("profile/setLastOpenedNotifications")]
        public async Task<IActionResult> SetLastOpenedNotifications([FromBody] ProfileLastOpenedUpdate requestBody)
        {
            var userProfile = await profileService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            DateTime lastOpenedTime = requestBody.LastOpened.Adapt<DateTime>();
            await profileService.SetLastOpenedNotifications(userProfile.Id, lastOpenedTime);

            return Ok();
        }

        [HttpPost("profile/setLastLogin")]
        public async Task<IActionResult> SetLastLogin([FromBody] ProfileLastLoginUpdate requestBody)
        {
            var userProfile = await profileService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            DateTime lastLogin = requestBody.LoginDateTime.Adapt<DateTime>();
            await profileService.SetLastLogin(userProfile.Id, lastLogin);

            return Ok();
        }
    }
}
