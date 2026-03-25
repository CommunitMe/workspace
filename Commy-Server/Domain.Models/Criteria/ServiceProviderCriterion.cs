using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
	public class ServiceProviderCriterion : ICriterion<ServiceProvider>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(ServiceProvider entity)
		{
			return entity.Id == Id;
		}
	}
}
