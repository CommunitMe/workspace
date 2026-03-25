using Mapster;

namespace WebAPI.ViewModels
{
    public class MarketItemCreateRequest
    {
        public string Owner { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Community { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        [AdaptIgnore]
        public string[] Tags { get; set; } = null!;
        public string ImageURI { get; set; } = string.Empty;
    }
}
