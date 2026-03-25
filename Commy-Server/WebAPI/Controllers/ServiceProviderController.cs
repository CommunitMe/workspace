using Domain.Services.Abstractions;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/service_provider")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class ServiceProviderController : Controller
    {
        private readonly ILogger<ProfilesController> logger;
        private readonly IServiceProviderService serviceProviderService;
        private readonly IProfilesService profilesService;

        public ServiceProviderController(ILogger<ProfilesController> logger, IServiceProviderService serviceProviderService, IProfilesService profileService)
        {
            this.logger = logger;
            this.serviceProviderService = serviceProviderService;
            this.profilesService = profileService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetServiceProvider(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var data = await serviceProviderService.GetServiceProviderDataAsync(id);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", id);
                return UnprocessableEntity();
            }

            var response = data.Adapt<ServiceProviderData>();

            if (data.ImageUid != null)
            {
                response.ImageURI = $"{Request.Scheme}://{Request.Host}/communitme/api/image/ServiceProvider/{data.ImageUid}";
            }

            return Ok(response);
        }
    }
}
