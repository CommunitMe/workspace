namespace Domain.DTOs
{
    public class Community
    {
        public long Cid { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageURI { get; set; } = string.Empty;
    }
}
