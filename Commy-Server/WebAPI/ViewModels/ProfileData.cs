using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptFrom(typeof(Domain.Models.Profile))]
    public class ProfileData
    {
        public string Id { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string DefaultCommunity { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string FamilyName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? State { get; set; }
        public string PostalCode { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.Profile.PhoneNumber))]
        public string Phone { get; set; } = string.Empty;
        public long Birthday { get; set; }
        [AdaptMember(nameof(Domain.Models.Profile.LastOpenNotifications))]
        public long LastOpenedNotifications { get; set; }
        public string ImageURI { get; set; } = string.Empty;
        [AdaptMember(nameof(Domain.Models.Profile.LastLogin))]
        public long? LastLogin { get; set; }
        public List<CommunityProfile> CommunityProfiles { get; set; } = new List<CommunityProfile>();
        [AdaptMember(nameof(Domain.Models.Profile.ServiceProvider))]
        public string? ServiceProviderId { get; set; }
    }
}
