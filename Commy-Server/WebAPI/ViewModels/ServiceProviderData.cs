using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptFrom(typeof(Domain.Models.ServiceProvider))]
    public class ServiceProviderData
    {
        public string Id { get; set; } = string.Empty;
        public int ServiceProviderType { get; set; }
        public string? Name { get; set; } = string.Empty;
        public string? ImageURI { get; set; } = string.Empty;
        public string? Phone { get; set; }
		public string? Email { get; set; }
        [AdaptMember(nameof(Domain.Models.ServiceProvider.ServiceTypes))]
        public ICollection<ServiceTypeData> ServiceTypes { get; set; } = new List<ServiceTypeData>();
    }
}
