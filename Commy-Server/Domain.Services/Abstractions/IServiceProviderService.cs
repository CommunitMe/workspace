using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface IServiceProviderService
    {
        Task<ServiceProvider> GetServiceProviderDataAsync(long id);
        Task<List<ServiceProvider>> GetAll();
    }
}
