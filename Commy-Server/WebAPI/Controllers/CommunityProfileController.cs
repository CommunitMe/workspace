using Domain.Models;
using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/community_profile")]
	[Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class CommunityProfileController : Controller
    {
        private readonly ILogger<CommunityController> logger;
        private readonly ICommunityProfileService communityProfileService;

        public CommunityProfileController(
            ILogger<CommunityController> logger,
            ICommunityProfileService communityProfileService)
        {
            this.logger = logger;
            this.communityProfileService = communityProfileService;
        }

        [HttpPost("update_member_state")]
        public async Task<IActionResult> UpdateMemberState([FromBody] CommunityProfile communityProfile)
        {
            await communityProfileService.UpdateMemberState(communityProfile);
            return Ok();
        }

        [HttpPost("update_provider_state")]
        public async Task<IActionResult> UpdateProviderState([FromBody] CommunityProfile communityProfile)
        {
            await communityProfileService.UpdateProviderState(communityProfile);
            return Ok();
        }
    }
}