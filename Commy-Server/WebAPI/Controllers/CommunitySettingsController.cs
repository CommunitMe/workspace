using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels;
using Mapster;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/community_settings")]
	[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class CommunitySettingsController : Controller
    {
        private readonly ILogger<CommunitySettingsController> logger;
        private readonly ICommunitySettingsService communitySettingsService;

        public CommunitySettingsController(
            ILogger<CommunitySettingsController> logger,
            ICommunitySettingsService communitySettingsService)
        {
            this.logger = logger;
            this.communitySettingsService = communitySettingsService;
        }

        [HttpGet("by_community/{cid}")]
        public async Task<IActionResult> GetCommunitySettingsByCommunityId(long cid)
        {
            var communitySettings = await communitySettingsService.GetCommunitySettingsByCommunityId(cid);

            if (communitySettings == null)
            {
                logger.LogInformation("No community settings for community with id '{0}'", cid);
                return Ok(null);
            }

            return Ok(communitySettings.Adapt<CommunitySettings>());
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCommunitySettings([FromBody] CommunitySettingsCreateRequest model)
        {
            var communitySettings = model.Adapt<Domain.Models.CommunitySettings>();

            communitySettings = await communitySettingsService.CreateCommunitySettings(communitySettings);

            return Ok(communitySettings.Adapt<CommunitySettings>());
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateCommunitySettings([FromBody] CommunitySettings model)
        {
            var communitySettings = model.Adapt<Domain.Models.CommunitySettings>();

            await communitySettingsService.UpdateCommunitySettings(communitySettings);

            return Ok();
        }
    }
}