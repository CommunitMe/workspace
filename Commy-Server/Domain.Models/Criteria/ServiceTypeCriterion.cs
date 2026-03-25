using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class ServiceTypeCriterion : ICriterion<ServiceType>
    {
        public long Id { get; set; }

        public object[] GetKeys()
        {
            return new object[]
            {
                Id
            };
        }

        public bool IsMet(ServiceType entity)
        {
            return entity.Id == Id;
        }
    }
}