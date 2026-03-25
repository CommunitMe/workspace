using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/languages")]
    [AllowAnonymous]
    public class LanguagesController : Controller
    {
        private readonly ILogger<LanguagesController> logger;
        private readonly ILanguageService languageService;
        private readonly IProfilesService profilesService;
        
        public LanguagesController(ILogger<LanguagesController> logger, ILanguageService languageService, IProfilesService profilesService)
        {
            this.logger = logger;
            this.languageService = languageService;
            this.profilesService = profilesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLanguages()
        {
            var languages = await languageService.GetLanguages();

            if (languages == null)
            {
                logger.LogError("Failed to get languages");
                return UnprocessableEntity();
            }

            return Ok(languages);
        }
    }
}
