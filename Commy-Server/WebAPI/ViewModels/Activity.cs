namespace WebAPI.ViewModels
{
    public class Activity
    {
        public long Timestamp { get; set; }
        public string ImageURI { get; set; } = string.Empty;
        public string ActivityType { get; set; } = string.Empty;
        public ActivityParams ActivityParams { get; set; } = null!;
    }
}
