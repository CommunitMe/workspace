using Domain.Abstractions.Common;

namespace Domain.Models.Criteria
{
    public class CategoryCriterion : ICriterion<Category>
    {
        public long Id { get; set; }

        public object[] GetKeys()
        {
            return new object[]
            {
                Id
            };
        }

        public bool IsMet(Category entity)
        {
            return entity.Id == Id;
        }
    }
}
