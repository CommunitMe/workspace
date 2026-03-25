using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class ServiceType : IPersistantModel<ServiceType>
	{
		public long Id { get; set; }
		public string Name { get; set; } = string.Empty;
		public long Category { get; set; }
		public Category? CategoryNavigation { get; set; }

		public ICriterion<ServiceType> GetCriterion()
		{
			return new ServiceTypeCriterion { Id = Id };
		}
	}
}
