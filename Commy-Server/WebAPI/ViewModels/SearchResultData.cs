namespace WebAPI.ViewModels
{
    public class SearchResultData
    {
        public int EntityTypeId { get; set; }
        public List<string> EntityIds { get; set; } = new List<string>();
    }
}
