using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
    public class ServiceTypeRepository : IServiceTypeRepository
    {
        private readonly CmeDbContext _db;
        public ServiceTypeRepository(CmeDbContext dbContext)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task<ServiceType> Create(ServiceType model)
        {
            throw new NotImplementedException();
        }

        public async Task Delete(ServiceType model)
        {
            ICriterion<ServiceType>? criterion = model.GetCriterion();
            var row = await _db.ServiceTypes.FindAsync(criterion.GetKeys());

            if (row == null)
                throw new NullReferenceException($"row of '{typeof(ServiceType).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

            _db.Remove(_db.ServiceTypes.Entry(row));
            await _db.SaveChangesAsync();
        }

        public Task<List<ServiceType>> GetAll(ICriterion<ServiceType>? criterion)
        {
            var query = _db.ServiceTypes.Select(m => m.Adapt<ServiceType>());

            if (criterion != null)
            {
                query = query.Where(row => criterion.IsMet(row));
            }

            return Task.FromResult(query.ToList());
        }

        public async Task<ServiceType?> GetById(ICriterion<ServiceType> criterion)
        {
            var result = await _db.ServiceTypes.FindAsync(criterion.GetKeys());

            if (result == null)
                return null;

            await _db.Entry(result).Reference(reference => reference.CategoryNavigation).LoadAsync();

            return result.Adapt<ServiceType>();
        }

        public Task<ServiceType> Update(ServiceType model)
        {
            throw new NotImplementedException();
        }
    }
}