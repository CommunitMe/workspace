using Domain.Abstractions.Common;
using Domain.Models.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class CommunityProfile : IPersistantModel<CommunityProfile>, IEquatable<CommunityProfile>
    {
        public long CommunityId { get; set; }
        public long ProfileId { get; set; }
        public int MemberState { get; set; }
        public int ProviderState { get; set; }

        public override bool Equals(object? obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(CommunityProfile? other)
        {
            return other != null &&
                other.CommunityId == CommunityId &&
                other.ProfileId == ProfileId;
        }

        public ICriterion<CommunityProfile> GetCriterion()
        {
            return new CommunityProfileCriterion { CommunityId = CommunityId, ProfileId = ProfileId };
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CommunityId, ProfileId);
        }

        public static bool operator == (CommunityProfile? left, CommunityProfile? right)
        {
            return EqualityComparer<CommunityProfile>.Default.Equals(left, right);
        }

        public static bool operator != (CommunityProfile? left, CommunityProfile? right)
        {
            return !(left == right);
        }
    }
}
