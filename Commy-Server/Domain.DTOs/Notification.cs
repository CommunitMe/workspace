namespace Domain.DTOs
{
    public class Notification
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public long CommunityId { get; set; }
        public long DateInserted { get; set; }
    }
}
