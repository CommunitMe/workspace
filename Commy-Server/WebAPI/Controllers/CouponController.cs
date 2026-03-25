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
    [ApiController, Route("communitme/api/coupon")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class CouponController : Controller
    {
        private readonly ILogger<CouponController> logger;
        private readonly ICouponService couponService;
        private readonly ICommunityService communityService;
        private readonly IProfilesService profilesService;
        private readonly ITagService tagService;
        private readonly IImageService imageService;

        public CouponController(
            ILogger<CouponController> logger,
            ICouponService couponService,
            ICommunityService communityService,
            IProfilesService profilesService,
            ITagService tagService,
            IImageService imageService)
        {
            this.logger = logger;
            this.couponService = couponService;
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
                logger.LogError("Profile '{0}' has no permissions to view coupons for community '{1}'", userProfile.Id, community.Id);
                return Unauthorized();
            }

            var communityCoupons = await couponService.GetCouponsByCommunityId(cid);

            if (communityCoupons == null)
            {
                logger.LogError("Failed to get coupons for community with id '{0}'", cid);
                return UnprocessableEntity();
            }

            var result = communityCoupons.Select(i => i.Id);

            if (max != null)
            {
                result = result.Take(max.Value);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCoupon(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var coupon = await couponService.GetCouponById(id);

            if (coupon == null)
            {
                logger.LogError("Failed to get coupon id '{0}'", id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(coupon.RelevantCommunity);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", coupon.RelevantCommunity);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view coupon '{1}'", userProfile.Id, coupon.Id);
                return Unauthorized();
            }

            var result = coupon.Adapt<ViewModels.Coupon>();
            result.Extra = GetCouponExtraData(coupon);
            result.Tags = coupon.Tags.Select(t => t.Text).ToArray();
            result.ImageURI = ImageController.GetImageGetURI(Request, ImageLocationType.Coupon, coupon.ImageUid);

            return Ok(result);
        }

        private CouponExtraData? GetCouponExtraData(Domain.Models.Coupon coupon)
        {
            CouponExtraData extra = new WebAPI.ViewModels.CouponExtraData();

            if (coupon.Amount != null)
            {
                extra.Units = coupon.Amount;
                extra.Type = "sold-count";
                return extra;
            }

            if (coupon.LocationName != null)
            {
                extra.Name = coupon.LocationName;
                extra.Type = "location";
                return extra;
            }

            if (coupon.Expiration != null)
            {
                extra.Left = (int)(coupon.Expiration.Value - DateTime.Now.ToUniversalTime()).TotalDays + 1;
                extra.Type = "limited-time";
                return extra;
            }

            return null;
        }

        private void SetCouponExtraData(CouponExtraData? extraData, Domain.Models.Coupon coupon)
        {
            if (extraData == null)
            {
                return;
            }

            coupon.Amount = extraData.Units;
            coupon.LocationName = extraData.Name;

            if (extraData.Left.HasValue)
            {
                coupon.Expiration = DateTime.Today.AddDays(extraData.Left.Value);
            }
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCoupon([FromBody] WebAPI.ViewModels.CouponCreateRequest couponData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var model = couponData.Adapt<Domain.Models.Coupon>();
            var tags = await tagService.GetOrCreateTextTags(couponData.Tags);
            model.Tags = tags.ToList();
            model.ImageUid = couponData.ImageURI.Split('/').Last();
            SetCouponExtraData(couponData.Extra, model);
            model = await couponService.CreateCoupon(model);

            if (model == null)
            {
                logger.LogError("Failed to create Coupon");
                return UnprocessableEntity();
            }

            logger.LogInformation("New Coupon created with id '{0}'", model.Id);

            return Ok(model.Id);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateCoupon([FromBody] WebAPI.ViewModels.Coupon couponData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var oldCouponData = await couponService.GetCouponById(long.Parse(couponData.Id));

            if (oldCouponData == null)
            {
                logger.LogError("Failed to get Coupon id '{0}'", couponData.Id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(oldCouponData.RelevantCommunity);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", oldCouponData.RelevantCommunity);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view Coupon '{1}'", userProfile.Id, oldCouponData.Id);
                return Unauthorized();
            }

            var newCouponData = couponData.Adapt<Domain.Models.Coupon>();
            var tags = await tagService.GetOrCreateTextTags(couponData.Tags);
            newCouponData.Tags = tags.ToList();
            newCouponData.ImageUid = couponData.ImageURI.Split('/').Last();
            SetCouponExtraData(couponData.Extra, newCouponData);

            if (oldCouponData.RelevantCommunity != newCouponData.RelevantCommunity)
            {
                var newCommunity = await communityService.GetCommunityById(newCouponData.RelevantCommunity);

                if (newCommunity == null)
                {
                    logger.LogError("Failed to get community with id '{0}'", newCouponData.RelevantCommunity);
                    return UnprocessableEntity();
                }

                if (!await communityService.IsCommunityVisibleToProfile(userProfile, newCommunity))
                {
                    logger.LogError("Profile '{0}' has no permissions to view Coupon '{1}'", userProfile.Id, newCouponData.Id);
                    return Unauthorized();
                }
            }

            await couponService.UpdateCoupon(newCouponData);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCoupon(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var CouponData = await couponService.GetCouponById(id);

            if (CouponData == null)
            {
                logger.LogError("Failed to get Coupon id '{0}'", id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(CouponData.RelevantCommunity);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", CouponData.RelevantCommunity);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions for Coupon '{1}'", userProfile.Id, CouponData.Id);
                return Unauthorized();
            }

            await couponService.DeleteCoupon(CouponData);

            return Ok();
        }

        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCouponImage(long id, [FromQuery(Name = "w")] int? width, [FromQuery(Name = "h")] int? height)
        {
            if (id == 0)
            {
                logger.LogError("Recieved id 0");
                return UnprocessableEntity();
            }

            var data = await couponService.GetCouponById(id);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", id);
                return UnprocessableEntity();
            }

            Stream? imageStream = imageService.GetImageByUID(Domain.Models.Enums.ImageLocationType.Coupon, data.ImageUid, width, height);

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
