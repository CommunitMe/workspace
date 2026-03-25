using Domain.Models.Enums;
using Domain.Services.Abstractions;
using IdentityServer4;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/search")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class SearchController : Controller
    {
        private readonly ILogger<SearchController> logger;
        private readonly ISearchService searchService;
        private readonly IProfilesService profilesService;

        public SearchController(ILogger<SearchController> logger, IProfilesService profilesService, ISearchService searchService)
        {
            this.logger = logger;
            this.searchService = searchService;
            this.profilesService = profilesService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetSearchReults([FromQuery] string entityTypeIds, [FromQuery] string searchTerm, [FromQuery] long cid, [FromQuery] string? scKeys)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            List<EntityType> entityTypes = new List<EntityType>();
            foreach(var id in entityTypeIds.Split(","))
            {
                try
                {
                    entityTypes.Add((EntityType)int.Parse(id));
                }
                catch(Exception ex)
                {
                    logger.LogError("failed parsing entity type id", ex);
                }              
            }

            List<SearchResultData> result = new List<SearchResultData>();

            var searchResults = await this.searchService.GetSearchResults(searchTerm, entityTypes, cid, scKeys);
            foreach (var searchResult in searchResults)
            {
                result.Add(new SearchResultData
                {
                    EntityTypeId = searchResult.EntityTypeId,
                    EntityIds = searchResult.EntityIds.Select(id => id.ToString()).ToList(),
                });
            }

            return Ok(result);
        }
    }
}
