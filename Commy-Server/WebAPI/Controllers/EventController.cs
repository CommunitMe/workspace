using Domain.Models.Enums;
using Domain.Services;
using Domain.Services.Abstractions;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System.Security.Cryptography;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/event")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class EventController : Controller
    {
        private readonly ILogger<EventController> logger;
        private readonly IEventService eventService;
        private readonly ICommunityService communityService;
        private readonly IProfilesService profilesService;
        private readonly IImageService imageService;

        public EventController(
            ILogger<EventController> logger,
            IEventService eventService,
            ICommunityService communityService,
            IProfilesService profilesService,
            IImageService imageService)
        {
            this.logger = logger;
            this.eventService = eventService;
            this.communityService = communityService;
            this.profilesService = profilesService;
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
                logger.LogError("Profile '{0}' has no permissions to view events for community '{1}'", userProfile.Id, community.Id);
                return Unauthorized();
            }

            var communityEvents = await eventService.GetEventByCommunityId(cid);

            if (communityEvents == null)
            {
                logger.LogError("Failed to get events for community with id '{0}'", cid);
                return UnprocessableEntity();
            }

            var result = communityEvents.Select(i => i.Id);

            if (max != null)
            {
                result = result.Take(max.Value);
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var eventData = await eventService.GetEventById(id);

            if (eventData == null)
            {
                logger.LogError("Failed to get event id '{0}'", id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(eventData.RelevantCommunity);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", eventData.RelevantCommunity);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view event '{1}'", userProfile.Id, eventData.Id);
                return Unauthorized();
            }

            var result = eventData.Adapt<WebAPI.ViewModels.Event>();
            result.Interested = new WebAPI.ViewModels.InterestedProfilesData
            {
                Count = eventData.Interested.Count,
                Uids = eventData.Interested.Select(pid => pid.ToString()).ToArray()
            };

            result.ImageURI = ImageController.GetImageGetURI(Request, ImageLocationType.Event, eventData.ImageUid);

            return Ok(result);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateEvent([FromBody] WebAPI.ViewModels.EventCreateRequest eventData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var model = eventData.Adapt<Domain.Models.Event>();
            model.ImageUid = eventData.ImageURI.Split('/').Last();
            model = await eventService.CreateEvent(model);

            if (model == null)
            {
                logger.LogError("Failed to create event");
                return UnprocessableEntity();
            }

            logger.LogInformation("New event created with id '{0}'", model.Id);

            return Ok(model.Id);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateEvent([FromBody] WebAPI.ViewModels.Event eventData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var oldEventData = await eventService.GetEventById(long.Parse(eventData.Eid));

            if (oldEventData == null)
            {
                logger.LogError("Failed to get event id '{0}'", eventData.Eid);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(oldEventData.RelevantCommunity);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", oldEventData.RelevantCommunity);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions to view event '{1}'", userProfile.Id, oldEventData.Id);
                return Unauthorized();
            }

            var newEventData = eventData.Adapt<Domain.Models.Event>();
            newEventData.ImageUid = eventData.ImageURI.Split('/').Last();
            newEventData.Interested = eventData.Interested.Uids.Select(uid => long.Parse(uid)).ToList();

            if (oldEventData.RelevantCommunity != newEventData.RelevantCommunity)
            {
                var newCommunity = await communityService.GetCommunityById(newEventData.RelevantCommunity);

                if (newCommunity == null)
                {
                    logger.LogError("Failed to get community with id '{0}'", newEventData.RelevantCommunity);
                    return UnprocessableEntity();
                }

                if (!await communityService.IsCommunityVisibleToProfile(userProfile, newCommunity))
                {
                    logger.LogError("Profile '{0}' has no permissions to view event '{1}'", userProfile.Id, newEventData.Id);
                    return Unauthorized();
                }
            }

            await eventService.UpdateEvent(newEventData);

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(long id)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var eventData = await eventService.GetEventById(id);

            if (eventData == null)
            {
                logger.LogError("Failed to get event id '{0}'", id);
                return UnprocessableEntity();
            }

            var community = await communityService.GetCommunityById(eventData.RelevantCommunity);

            if (community == null)
            {
                logger.LogError("Failed to get community with id '{0}'", eventData.RelevantCommunity);
                return UnprocessableEntity();
            }

            if (!await communityService.IsCommunityVisibleToProfile(userProfile, community))
            {
                logger.LogError("Profile '{0}' has no permissions for event '{1}'", userProfile.Id, eventData.Id);
                return Unauthorized();
            }

            await eventService.DeleteEvent(eventData);

            return Ok();
        }


        [HttpGet("{id}/image")]
        [AllowAnonymous]
        public async Task<IActionResult> GetEventImage(long id, [FromQuery(Name = "w")] int? width, [FromQuery(Name = "h")] int? height)
        {
            if (id == 0)
            {
                logger.LogError("Recieved id 0");
                return UnprocessableEntity();
            }

            var data = await eventService.GetEventById(id);

            if (data == null)
            {
                logger.LogError("Failed to find id {0}", id);
                return UnprocessableEntity();
            }

            Stream? imageStream = imageService.GetImageByUID(ImageLocationType.Event, data.ImageUid, width, height);

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
