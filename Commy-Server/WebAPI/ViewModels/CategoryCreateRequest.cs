using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.Category))]
    public class CategoryCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ImageURI { get; set; } = string.Empty;
    }
}
