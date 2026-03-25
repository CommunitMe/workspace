using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
    public class Category : IPersistantModel<Category>
    {
        public long Id { get; set; }

        public string Name { get; set; } = null!;
        public string? ImageUid { get; set; }

        public long? Parent { get; set; }

        public ICriterion<Category> GetCriterion()
        {
            return new CategoryCriterion { Id = Id };
        }
    }
}
