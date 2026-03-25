using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class ServiceProvider : IPersistantModel<ServiceProvider>
	{
		public long Id { get; set; }

		public int ServiceProviderType { get; set; }

		public string? Name { get; set; }

        public string? ImageUid { get; set; }
		
		public string? Phone { get; set; }

		public string? Email { get; set; }

		public ICollection<ServiceType> ServiceTypes { get; set; } = new List<ServiceType>();

        public ICriterion<ServiceProvider> GetCriterion()
		{
			return new ServiceProviderCriterion { Id = Id };
		}
	}
}
