namespace WebAPI.ViewModels
{
    public class CommunityProfile
    {
        public string CommunityId { get; set; } = string.Empty;
        public string ProfileId { get; set; } = string.Empty;
        public int MemberState { get; set; }
        public int ProviderState { get; set; }
    }
}
