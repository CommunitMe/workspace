namespace Domain.Models
{
    public class SearchResult
    {
        public int EntityTypeId { get; set; }
        public List<long> EntityIds { get; set; } = new List<long>();
    }
}
