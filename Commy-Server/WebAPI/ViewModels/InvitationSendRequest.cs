namespace WebAPI.ViewModels
{
    public class InvitationSendRequest
    {
        public int InvitationType { get; set; }
        public string CommunityId { get; set; } = string.Empty;
        public string? Emails { get; set; } = string.Empty;
        public string? PhoneNumbers { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}