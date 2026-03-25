using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class InvitationService : IInvitationService
    {
        private readonly ILogger<InvitationService> logger;
		private readonly IDatabaseTransaction db;

        public InvitationService(ILogger<InvitationService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<List<Invitation>?> GetInvitationsByCommunityId(long cid, int? maxAmount = null)
        {
            var filter = new InvitationByCommunityCriterion { CommunityId = cid };

            IEnumerable<Invitation> result = await db.Invitations.GetAll(filter);

            if (maxAmount.HasValue)
            {
                result = result.Take(maxAmount.Value);
            }

            return result.ToList();
        }

        public async Task<List<Invitation>?> GetInvitationsByUserId(long uid, int? maxAmount = null)
        {
            var filter = new InvitationByUserCriterion { UserId = uid };

            IEnumerable<Invitation> result = await db.Invitations.GetAll(filter);

            if (maxAmount.HasValue)
            {
                result = result.Take(maxAmount.Value);
            }

            return result.ToList();
        }

        public async Task<Invitation?> GetInvitationById(Guid id)
        {
            var invitation = await db.Invitations.GetById(new InvitationCriterion { Id = id });

            if (invitation == null)
            {
                this.logger.LogInformation(message: "No invitation item with id '{0}'", id);
            }

            return invitation;
        }

        public async Task<Invitation> CreateInvitation(Invitation invitationData)
        {
            invitationData.Id = Guid.NewGuid();
            invitationData = await db.Invitations.Create(invitationData);
            await db.SaveChangesAsync();
            return invitationData;
        }

        public async Task<Invitation> UpdateInvitation(Invitation invitationData)
        {
            invitationData = await db.Invitations.Update(invitationData);
            await db.SaveChangesAsync();
            return invitationData;
        }

        public async Task DeleteInvitation(Invitation invitationData)
        {
            await db.Invitations.Delete(invitationData);
            await db.SaveChangesAsync();
        }
    }
}