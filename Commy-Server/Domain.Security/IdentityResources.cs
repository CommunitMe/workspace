using IdentityServer4.Models;
using IS4 = IdentityServer4.Models;

namespace Domain.Security
{
	public static class IdentityResources
	{
		public static IdentityResource[] AllResources => new[] {
			_openIdResource,
			_profileResource
		};

		private static IdentityResource _openIdResource = new IS4.IdentityResources.OpenId();
		private static IdentityResource _profileResource = new IS4.IdentityResources.Profile();
	}
}
