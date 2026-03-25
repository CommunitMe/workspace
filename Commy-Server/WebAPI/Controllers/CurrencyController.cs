using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/currencies")]
    [AllowAnonymous]
    public class CurrencyController : Controller
    {
        private readonly ILogger<CurrencyController> logger;
        private readonly ICurrencyService currencyService;
        private readonly IProfilesService profilesService;

        public CurrencyController(ILogger<CurrencyController> logger, ICurrencyService currencyService, IProfilesService profilesService)
        {
            this.logger = logger;
            this.currencyService = currencyService;
            this.profilesService = profilesService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencies()
        {
            var currencies = await currencyService.GetCurrencies();

            if (currencies == null)
            {
                logger.LogError("Failed to get currencies");
                return UnprocessableEntity();
            }

            return Ok(currencies);
        }
    }
}
