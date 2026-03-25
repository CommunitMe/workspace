using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
    public class ServiceTypeService : IServiceTypeService
    {
        private readonly ILogger<ServiceProviderService> logger;
        private readonly IDatabaseTransaction db;


        public ServiceTypeService(ILogger<ServiceProviderService> logger, IDatabaseTransaction db)
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<ServiceType> GetServiceTypeDataAsync(long id)
        {
            var serviceType = await db.ServiceTypes.GetById(new ServiceTypeCriterion { Id = id });

            if (serviceType == null)
            {
                this.logger.LogInformation("No service type with id '{0}'", id);
            }
            
            return serviceType;
        }

        public async Task<List<ServiceType>> GetAll()
        {
            return await db.ServiceTypes.GetAll();
        }
    }
}