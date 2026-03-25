namespace Domain.DTOs
{
    public class Product
    {
        public long Id { get; set; }
        public long Owner { get; set; }
        public long Community { get; set; }
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string[] Tags { get; set; } = null!;
        public string ImageURI { get; set; } = string.Empty;
    }
}
