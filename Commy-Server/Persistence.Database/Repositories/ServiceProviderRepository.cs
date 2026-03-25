using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;

namespace Persistence.Database.Repositories
{
	public class ServiceProviderRepository : IServiceProviderRepository
    {
		private readonly CmeDbContext _db;

		public ServiceProviderRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public Task<ServiceProvider> Create(ServiceProvider model)
		{
			throw new NotImplementedException();
		}

		public async Task Delete(ServiceProvider model)
		{
			ICriterion<ServiceProvider>? criterion = model.GetCriterion();
			var row = await _db.ServiceProviders.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{typeof(ServiceProvider).Name}' by criterion '{criterion}' does not exist in DatabaseContext");

			_db.Remove(_db.ServiceProviders.Entry(row));
			await _db.SaveChangesAsync();
		}

		public Task<List<ServiceProvider>> GetAll(ICriterion<ServiceProvider>? criterion)
		{
			var query = _db.ServiceProviders.Select(m => m.Adapt<Domain.Models.ServiceProvider>());

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<ServiceProvider?> GetById(ICriterion<ServiceProvider> criterion)
		{
			var result = await _db.ServiceProviders.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			await _db.Entry(result).Collection(reference => reference.ServiceTypes).LoadAsync();
			foreach (var serviceType in result.ServiceTypes)
			{
				await _db.Entry(serviceType).Reference(reference => reference.CategoryNavigation).LoadAsync();
			}

			return result.Adapt<Domain.Models.ServiceProvider>();
		}

		public Task<ServiceProvider> Update(ServiceProvider model)
		{
			throw new NotImplementedException();
		}
	}
}
