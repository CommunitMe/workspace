using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
    public class Profile : IPersistantModel<Profile>
    {
        public long Id { get; set; }

        public string? UserName { get; set; }

        public string? NormalizedUserName { get; set; }

        public string GivenName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string FamilyName { get; set; } = null!;

        public string Address { get; set; } = null!;

        public string City { get; set; } = null!;

        public string? State { get; set; }

        public string PostalCode { get; set; } = null!;

        public DateTime Birthday { get; set; }

        public string PhoneNumber { get; set; } = null!;

        public bool PhoneNumberConfirmed { get; set; }

        public string Email { get; set; } = null!;

        public string? NormalizedEmail { get; set; }

        public bool EmailConfirmed { get; set; }

        public long DefaultCommunity { get; set; }

        public long? ServiceProvider { get; set; } = null;

        public string? ImageUid { get; set; }

        public DateTime LastOpenNotifications { get; set; } = DateTime.Now.ToUniversalTime();

        public DateTime? LastLogin { get; set; }

        public string? PasswordHash { get; set; }

        public string? SecurityStamp { get; set; }

        public string? ConcurrencyStamp { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public DateTimeOffset? LockoutEnd { get; set; }

        public bool LockoutEnabled { get; set; }

        public int AccessFailedCount { get; set; }

        public List<CommunityProfile> CommunityProfiles { get; set; } = new List<CommunityProfile>();

        public ServiceProvider? ServiceProviderNavigation { get; set; }

        public ICriterion<Profile> GetCriterion()
        {
            return new ProfileCriterion { Id = Id };
        }
    }
}
