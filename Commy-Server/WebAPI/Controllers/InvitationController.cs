using Domain.Infra.Globals;
using Domain.Models;
using Domain.Services.Abstractions;
using IdentityServer4;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController, Route("communitme/api/invitation")]
    [Authorize(IdentityServerConstants.LocalApi.PolicyName)]
    public class InvitationController : Controller
    {
        private readonly ILogger<InvitationController> logger;
        private readonly IInvitationService invitationService;
        private readonly IProfilesService profilesService;
        private readonly ISmsManager smsManager;
        public InvitationController(
            ILogger<InvitationController> logger,
            IInvitationService invitationService,
            IProfilesService profilesService,
            ISmsManager smsManager)
        {
            this.logger = logger;
            this.invitationService = invitationService;
            this.profilesService = profilesService;
            this.smsManager = smsManager;
        }
        
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] ViewModels.InvitationCreateRequest invitationData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            var now = DateTime.UtcNow;
            var invitation = invitationData.Adapt<Invitation>();
            invitation.CreationTime = now;
            invitation.Expiry = invitationData.ExpirationDays == 0 ? null : now.AddDays(invitationData.ExpirationDays);
            invitation.CreatorId = userProfile.Id;
            invitation = await invitationService.CreateInvitation(invitation);

            if (invitation == null)
            {
                logger.LogError("Failed to create invitation");
                return BadRequest("Failed to create invitation");
            }

            logger.LogInformation("New invitation created with id '{0}'", invitation.Id);

            return Ok($"https://communitme.com/account/register?data={invitation.Id}");
        }

        [HttpPost("send")]
        public async Task<IActionResult> Send([FromBody] ViewModels.InvitationSendRequest invitationData)
        {
            var userProfile = await profilesService.GetUserProfile(User);

            if (userProfile == null)
            {
                logger.LogError("Failed to get user profile for current claims");
                return Unauthorized("Invalid user");
            }

            if (!string.IsNullOrEmpty(invitationData.Emails))
            {
                var result = await SendEmailInvitations(invitationData, userProfile.Id);

                if (!result)
                {
                    logger.LogError("Failed to send email invitations");
                    return BadRequest("Failed to send email invitations");
                }
            }
            else
            {
                var result = await SendSmsInvitations(invitationData, userProfile.Id);

                if (!result)
                {
                    logger.LogError("Failed to send sms invitations");
                    return BadRequest("Failed to send sms invitations");
                }
            }

            return Ok();
        }

        private async Task<bool> SendEmailInvitations(ViewModels.InvitationSendRequest invitationData, long userId)
        {
            var emails = invitationData.Emails?.Split(',') ?? new string[0];

            foreach (var email in emails)
            {
                var invitationCreateData = new ViewModels.InvitationCreateRequest
                {
                    CommunityId = invitationData.CommunityId,
                    InvitationType = invitationData.InvitationType,
                    AllowedUsages = 1
                };
                var invitation = invitationCreateData.Adapt<Invitation>();
                invitation.CreationTime = DateTime.UtcNow;
                invitation.CreatorId = userId;
                invitation = await invitationService.CreateInvitation(invitation);

                if (invitation == null)
                {
                    logger.LogError("Failed to create invitation");
                    return false;
                }

                logger.LogInformation("New invitation created with id '{0}'", invitation.Id);

                var response = Mail.Send(email, "Invitation", $"{invitationData.Message} https://communitme.com/account/register?data={invitation.Id}");
                if (response.StatusCode == -1)
                {
                    logger.LogError("Failed to send email invitation to email {0} with exception {1}", email, response.Exception);
                    return false;
                }
            }

            return true;
        }

        private async Task<bool> SendSmsInvitations(ViewModels.InvitationSendRequest invitationData, long userId)
        {
            var phoneNumbers = invitationData.PhoneNumbers?.Split(',') ?? new string[0];

            foreach (var phoneNumber in phoneNumbers)
            {
                var invitationCreateData = new ViewModels.InvitationCreateRequest
                {
                    CommunityId = invitationData.CommunityId,
                    InvitationType = invitationData.InvitationType,
                    AllowedUsages = 1
                };
                var invitation = invitationCreateData.Adapt<Invitation>();
                invitation.CreationTime = DateTime.UtcNow;
                invitation.CreatorId = userId;
                invitation = await invitationService.CreateInvitation(invitation);

                if (invitation == null)
                {
                    logger.LogError("Failed to create invitation");
                    return false;
                }

                logger.LogInformation("New invitation created with id '{0}'", invitation.Id);

                smsManager.Send(phoneNumber, "CommunitMe", "Invitation", $"{invitationData.Message} https://communitme.com/account/register?data={invitation.Id}");
            }

            return true;
        }
    }
}