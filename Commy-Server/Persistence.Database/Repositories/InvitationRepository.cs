using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
    public class InvitationRepository : IInvitationRepository
    {
        private readonly CmeDbContext _db;

        public InvitationRepository(CmeDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task<Invitation> Create(Invitation model)
        {
            var data = model.Adapt<Entities.Invitation>();

            var result = _db.Invitations.Add(data);
            await _db.SaveChangesAsync();

            return result.Entity.Adapt<Domain.Models.Invitation>();
        }

        public async Task Delete(Invitation model)
        {
            ICriterion<Invitation>? criterion = model.GetCriterion();
            var row = await _db.Invitations.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(Invitation).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Remove(row);
			await _db.SaveChangesAsync();

        }

        public Task<List<Invitation>> GetAll(ICriterion<Invitation>? criterion = null)
        {
            var query = _db.Invitations.Select(e => e.Adapt<Domain.Models.Invitation>()).AsEnumerable();

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<Invitation?> GetById(ICriterion<Invitation> criterion)
        {
            var entity = await _db.Invitations.FindAsync(criterion.GetKeys());

            if (entity == null)
                return null;

            return entity.Adapt<Domain.Models.Invitation>();
        }

        public async Task<Invitation> Update(Invitation model)
        {
            ICriterion<Invitation>? criterion = model.GetCriterion();
            var entity = await _db.Invitations.FindAsync(criterion.GetKeys());

            if (entity == null)
                throw new NullReferenceException($"row of '{typeof(Invitation).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            var updatedEntity = model.Adapt<Entities.Invitation>();
            var result = _db.Invitations.Update(updatedEntity);
			await _db.SaveChangesAsync();
            return result.Entity.Adapt<Domain.Models.Invitation>();
        }
    }
}