using Domain.Models.Enums;
using Domain.Services;
using Domain.Services.Abstractions;
using IdentityServer4;
using IdentityServer4.Services;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/market-item")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class MarketItemController : Controller
    {
        private readonly ILogger<MarketItemController> logger;
        private readonly IMarketItemService marketItemService;
        private readonly ICommunityService communityService;
        private readonly IProfilesService profilesService;
        private readonly ITagService tagService;
        private readonly IImageService imageService;

        public MarketItemController(
            ILogger<MarketItemController> logger,
            IMarketItemService marketItemService,
            ICommunityService communityService,
            IProfilesService profilesService,
            ITagService tagService,
            IImageService imageService
            )
        {
            this.logger = logger;
            this.marketItemService = marketItemService;
            this.communityService = communityService;
            this.profilesService = profilesService;
            this.tagService = tagService;
            this.imageService = imageService;
        }

        [HttpGet("ids")]
        public async Task<IActionResult> GetIDs(long cid, int? max)
        {
            var userProfile = await profilesService.GetUserProfile(User);

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
                logger.LogError("Profile '{0}' has no permissions to view market items for community '{1}'", userProfile.Id, community.Id);
                return Unauthorized();
            }
            var communityItems = await marketItemService.GetMarketItemByCommunityId(cid);

            if (communityItems == null)
            {
                logger.LogError("Failed to get market items for community with id '{0}'", cid);
                return UnprocessableEntity();
            }

            var result = communityItems.Select(i => i.Id);

            if (max != null)
            {
                result = result.Take(max.Value);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMarketItem(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var marketItem = await marketItemService.GetMarketItemByID(id);

            if (marketItem == null)
            {
                logger.LogError("Failed to get market item id '{0}'", id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(marketItem.Community);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", marketItem.Community);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view market item '{1}'", userProfile.Id, marketItem.Id);
                return Unauthorized();
            }


            var result = marketItem.Adapt<ViewModels.MarketItem>();
            result.ImageURI = ImageController.GetImageGetURI(Request, ImageLocationType.MarketItem, marketItem.ImageUid);
            result.Tags = marketItem.Tags.Select(t => t.Text).ToArray();
            return Ok(result);
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetMarketItemImage(long id, [FromQuery(Name = "w")] int? width, [FromQuery(Name = "h")] int? height)
        {
            if (id == 0)
            {
                logger.LogError("Recieved id 0");
                return UnprocessableEntity();
            }

            var data = await marketItemService.GetMarketItemByID(id);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", id);
                return UnprocessableEntity();
            }

            Stream? imageStream = imageService.GetImageByUID(ImageLocationType.MarketItem, data.ImageUid, width, height);

            if (imageStream == null)
            {
                logger.LogError("Failed to get image for id {0}", id);
                return UnprocessableEntity();
            }

            var result = new FileStreamResult(imageStream, new MediaTypeHeaderValue("image/jpeg"));
            return result;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMarketItem([FromBody] WebAPI.ViewModels.MarketItemCreateRequest marketItemData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            marketItemData.Description = marketItemData.Description.Trim();

            if (String.IsNullOrWhiteSpace(marketItemData.Description))
            {
                logger.LogError("No description for market item");
                return UnprocessableEntity();
            }

            if (marketItemData.Description.Length < 3)
            {
                logger.LogError("Market Item description is too short");
                return UnprocessableEntity();
            }

            var model = marketItemData.Adapt<Domain.Models.MarketItem>();
            var tags = await tagService.GetOrCreateTextTags(marketItemData.Tags);
            model.Tags = tags.ToList();
            model.ImageUid = marketItemData.ImageURI.Split('/').Last();
            model.PriceCurrency = 840;
            model = await marketItemService.CreateMarketItem(model);

            if (model == null)
            {
                logger.LogError("Failed to create market item");
                return UnprocessableEntity();
            }

            logger.LogInformation("New market item created with id '{0}'", model.Id);

            return Ok(model.Id);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateMarketItem([FromBody] WebAPI.ViewModels.MarketItem marketItemData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var oldMarketItemData = await marketItemService.GetMarketItemByID(long.Parse(marketItemData.Id));

            if (oldMarketItemData == null)
            {
                logger.LogError("Failed to get market item id '{0}'", marketItemData.Id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(oldMarketItemData.Community);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", oldMarketItemData.Community);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view market item '{1}'", userProfile.Id, oldMarketItemData.Id);
                return Unauthorized();
            }

            if (String.IsNullOrWhiteSpace(marketItemData.Description))
            {
                logger.LogError("No description for market item");
                return UnprocessableEntity();
            }

            marketItemData.Description = marketItemData.Description.Trim();

            if (marketItemData.Description.Length < 3)
            {
                logger.LogError("Market item description is too short");
                return UnprocessableEntity();
            }

            var newMarketItemData = marketItemData.Adapt<Domain.Models.MarketItem>();
            var tags = await tagService.GetOrCreateTextTags(marketItemData.Tags);
            newMarketItemData.Tags = tags.ToList();
            newMarketItemData.ImageUid = marketItemData.ImageURI.Split('/').Last();
            newMarketItemData.PriceCurrency = 840;

            if (oldMarketItemData.Community != newMarketItemData.Community)
            {
                var newCommunity = await communityService.GetCommunityById(newMarketItemData.Community);

                if (newCommunity == null)
                {
                    logger.LogError("Failed to get community with id '{0}'", newMarketItemData.Community);
                    return UnprocessableEntity();
                }

                if (!await communityService.IsCommunityVisibleToProfile(userProfile, newCommunity))
                {
                    logger.LogError("Profile '{0}' has no permissions to view market item '{1}'", userProfile.Id, newMarketItemData.Id);
                    return Unauthorized();
                }
            }

            await marketItemService.UpdateMarketItem(newMarketItemData);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMarketItem(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var marketItemData = await marketItemService.GetMarketItemByID(id);

            if (marketItemData == null)
            {
                logger.LogError("Failed to get market item id '{0}'", id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(marketItemData.Community);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", marketItemData.Community);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions for market item '{1}'", userProfile.Id, marketItemData.Id);
                return Unauthorized();
            }

            await marketItemService.DeleteMarketItem(marketItemData);

            return Ok();
        }
    }
}
