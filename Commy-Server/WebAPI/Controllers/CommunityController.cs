using Domain.Models.Enums;
using Domain.Services.Abstractions;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using SkiaSharp;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
	[ApiController, Route("communitme/api/community")]
	[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
	public class CommunityController : Controller
	{
		private readonly ILogger<CommunityController> logger;
		private readonly ICommunityService communityService;
		private readonly IProfilesService profilesService;
		private readonly IImageService imageService;

		public CommunityController(
			ILogger<CommunityController> logger,
			ICommunityService communityService,
			IProfilesService profilesService,
			IImageService imageService)
		{
			this.logger = logger;
			this.communityService = communityService;
			this.profilesService = profilesService;
			this.imageService = imageService;
		}

		[HttpGet("ids")]
		public async Task<IActionResult> GetCurrentProfileCommunities()
		{
			var userProfile = await profilesService.GetUserProfile(User);

			if (userProfile == null)
			{
				logger.LogError("Failed to get user profile for current claims");
				return Unauthorized("Invalid user");
			}

			var profileCommunities = await communityService.GetCommunitiesByProfileId(userProfile.Id);

			var result = profileCommunities.Select(community => community.Id);
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetCommunity(long id)
		{
			var userProfile = await profilesService.GetUserProfile(User);

			if (userProfile == null)
			{
				logger.LogError("Failed to get user profile for current claims");
				return Unauthorized("Invalid user");
			}

			var community = await communityService.GetCommunityById(id);

			if (community == null)
			{
				logger.LogError("Failed to get community with id '{0}'", id);
				return UnprocessableEntity();
			}

			if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
			{
				logger.LogError("Profile '{0}' has no permissions to view community '{1}'", userProfile.Id, community.Id);
				return Unauthorized();
			}

			var result = community.Adapt<Community>();

			result.ImageURI = ImageController.GetImageGetURI(Request, ImageLocationType.Commmunity, community.ImageUid);

			return Ok(result);
		}

		[HttpGet("{id}/image")]
		[AllowAnonymous]
		public async Task<IActionResult> GetCommunityImage(long id, [FromQuery(Name = "w")] int? width, [FromQuery(Name = "h")] int? height)
		{
			if (id == 0)
			{
				logger.LogError("Recieved id 0");
				return UnprocessableEntity();
			}

			var data = await communityService.GetCommunityById(id);

			if (data == null)
			{
				logger.LogError("Failed to find id {0}", id);
				return UnprocessableEntity();
			}

			Stream? imageStream = imageService.GetImageByUID(Domain.Models.Enums.ImageLocationType.Commmunity, data.ImageUid, width, height);

			if (imageStream == null)
			{
				logger.LogError("Failed to get image for id {0}", id);
				return UnprocessableEntity();
			}

			var result = new FileStreamResult(imageStream, new MediaTypeHeaderValue("image/jpeg"));
			return result;
		}

		[HttpPost("create")]
		public async Task<IActionResult> CreateCommunity([FromBody]CommunityCreateRequest communityCreateRequest)
		{
			var userProfile = await profilesService.GetUserProfile(User);

			if (userProfile == null)
			{
				logger.LogError("Failed to get user profile for current claims");
				return Unauthorized("Invalid user");
			}

			if (communityCreateRequest == null)
			{
				logger.LogError("Missing required parameter '{0}'", nameof(communityCreateRequest));
				return UnprocessableEntity();
			}

			var newCommunityTemplate = communityCreateRequest.Adapt<Domain.Models.Community>();
			newCommunityTemplate.ImageUid = "07c6ec4b-5fe2-478d-8125-99318c66c525";
			var newCommunity = await communityService.CreateCommunity(userProfile, newCommunityTemplate);

			if (newCommunity == null)
			{
				logger.LogError("Failed to create community");
				return UnprocessableEntity();
			}

			return Ok();
		}
	}
}
