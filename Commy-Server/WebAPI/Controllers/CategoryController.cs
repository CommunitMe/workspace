using Domain.Models.Enums;
using Domain.Services;
using Domain.Services.Abstractions;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Persistence.Database.Entities;
using System.Security.Cryptography;
using WebAPI.ViewModels;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/category")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> logger;
        private readonly ICategoryService CategoryService;
        private readonly ICommunityService communityService;
        private readonly IProfilesService profilesService;
        private readonly ITagService tagService;
        private readonly IImageService imageService;

        public CategoryController(
            ILogger<CategoryController> logger,
            ICategoryService CategoryService,
            ICommunityService communityService,
            IProfilesService profilesService,
            ITagService tagService,
            IImageService imageService)
        {
            this.logger = logger;
            this.CategoryService = CategoryService;
            this.communityService = communityService;
            this.profilesService = profilesService;
            this.tagService = tagService;
            this.imageService = imageService;
        }

        [HttpGet("by-community")]
        public async Task<IActionResult> GetIDs(long cid)
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
                logger.LogError("Profile '{0}' has no permissions to view Categories for community '{1}'", userProfile.Id, community.Id);
                return Unauthorized();
            }

            var communityCategories = await CategoryService.GetAll();

            if (communityCategories == null)
            {
                logger.LogError("Failed to get Categories for community with id '{0}'", cid);
                return UnprocessableEntity();
            }

            var result = communityCategories.Select(i => i.Id);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategory(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var Category = await CategoryService.GetCategoryById(id);

            if (Category == null)
            {
                logger.LogError("Failed to get Category id '{0}'", id);
                return UnprocessableEntity();
            }

            var result = Category.Adapt<ViewModels.Category>();
            if (Category.ImageUid != null)
            {
                result.ImageURI = ImageController.GetImageGetURI(Request, ImageLocationType.Category, Category.ImageUid);
            }

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> Category([FromBody] WebAPI.ViewModels.CategoryCreateRequest categoryData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var model = categoryData.Adapt<Domain.Models.Category>();
            model.ImageUid = categoryData.ImageURI.Split('/').Last();
            model = await CategoryService.CreateCategory(model);

            if (model == null)
            {
                logger.LogError("Failed to create Category");
                return UnprocessableEntity();
            }

            logger.LogInformation("New Category created with id '{0}'", model.Id);

            return Ok(model.Id);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCategory([FromBody] WebAPI.ViewModels.Category categoryData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var oldCategoryData = await CategoryService.GetCategoryById(long.Parse(categoryData.Id));

            if (oldCategoryData == null)
            {
                logger.LogError("Failed to get Category id '{0}'", categoryData.Id);
                return UnprocessableEntity();
            }

            var newCategoryData = categoryData.Adapt<Domain.Models.Category>();
            newCategoryData.ImageUid = categoryData.ImageURI.Split('/').Last();
            await CategoryService.UpdateCategory(newCategoryData);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var categoryData = await CategoryService.GetCategoryById(id);

            if (categoryData == null)
            {
                logger.LogError("Failed to get Category id '{0}'", id);
                return UnprocessableEntity();
            }

            await CategoryService.DeleteCategory(categoryData);

            return Ok();
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCategoryImage(long id, [FromQuery(Name = "w")] int? width, [FromQuery(Name = "h")] int? height)
        {
            if (id == 0)
            {
                logger.LogError("Recieved id 0");
                return UnprocessableEntity();
            }

            var data = await CategoryService.GetCategoryById(id);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", id);
                return UnprocessableEntity();
            }

            Stream? imageStream = imageService.GetImageByUID(Domain.Models.Enums.ImageLocationType.Category, data.ImageUid, width, height);

            if (imageStream == null)
            {
                logger.LogError("Failed to get image for id {0}", id);
                return UnprocessableEntity();
            }

            var result = new FileStreamResult(imageStream, new MediaTypeHeaderValue("image/jpeg"));
            return result;
        }
    }
}
