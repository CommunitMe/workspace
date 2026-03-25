using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Models.Enums;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class ServiceProviderService : IServiceProviderService, ISearchProvider
    {
        private readonly ILogger<ServiceProviderService> logger;
        private readonly IDatabaseTransaction db;

        public ServiceProviderService(ILogger<ServiceProviderService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<ServiceProvider> GetServiceProviderDataAsync(long id)
        {
            var serviceProvider = await db.ServiceProviders.GetById(new ServiceProviderCriterion { Id = id });

            if (serviceProvider == null)
            {
                this.logger.LogInformation("No service provider with id '{0}'", id);
            }

            return serviceProvider;
        }

        public async Task<List<ServiceProvider>> GetAll()
        {
            return await db.ServiceProviders.GetAll();
        }

        public async Task<SearchResult?> GetSearchResults(string searchTerm, long communityId, string? scKeys)
        {
            var serviceProviders = (await GetAll())
                .Where(p => p.Name?.ToLower().Contains(searchTerm.ToLower()) ?? false)
                .ToList();

            if (serviceProviders.Count == 0)
            {
                return null;
            }

            return new SearchResult
            {
                EntityTypeId = EntityType.Businesses.ID,
                EntityIds = serviceProviders.Select(m => m.Id).ToList()
            };
        }

        public EntityType GetSearchResultType()
        {
            return EntityType.Businesses;
        }
    }
}
