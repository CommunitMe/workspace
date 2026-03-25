using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/community_summary")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class CommunitySummaryController : Controller
    {
        private readonly ILogger<CommunityController> logger;
        private readonly IProfilesService profilesService;

        public CommunitySummaryController(ILogger<CommunityController> logger, IProfilesService profilesService)
        {
            this.logger = logger;
            this.profilesService = profilesService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCommunitySummary(long id)
        {
            var profiles = await profilesService.GetProfilesByCommunityId(id);
            var profileIds = profiles.Select(profile => profile.Id).ToList();

            var result = new CommunitySummary
            {
                Id = id.ToString(),
                ProfilesCount = profileIds.Count,
                ProfilesIds = profileIds.Take(20).Select(pid => pid.ToString()).ToList()
            };
            return Ok(result);
        }
    }
}
