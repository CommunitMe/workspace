using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.Category))]
    public class Category
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ImageURI { get; set; }
    }
}
