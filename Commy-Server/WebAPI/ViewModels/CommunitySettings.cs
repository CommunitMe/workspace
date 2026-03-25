using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.CommunitySettings))]
    public class CommunitySettings
    {
        public long Id { get; set; }

        public long CommunityId { get; set; }

        public string DefaultCurrency { get; set; } = null!;

        public string DefaultLanguage { get; set; } = null!;

        public int Privacy { get; set; }

        public string? CommunityUsername { get; set; }

        public int MembersApprovalPreferences { get; set; }

        public int SuppliersApprovalPreferences { get; set; }

        public string? Guidelines { get; set; }

        public int AgeRestriction { get; set; }
    }
}