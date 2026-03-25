using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptFrom(typeof(Domain.Models.ServiceType))]
    public class ServiceTypeData
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.ServiceType.CategoryNavigation))]
        public Category? Category { get; set; }
    }
}