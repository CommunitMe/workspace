using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface IServiceTypeService
    {
        Task<ServiceType> GetServiceTypeDataAsync(long id);
        Task<List<ServiceType>> GetAll();
    }
}