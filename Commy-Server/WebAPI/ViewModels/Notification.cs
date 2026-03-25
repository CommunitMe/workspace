using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.Notification))]
    public class Notification
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.Notification.RelevantCommunity))]
        public string CommunityId { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.Notification.InsertTime))]
        public long DateInserted { get; set; }
    }
}
