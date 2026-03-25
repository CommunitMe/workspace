using IdentityServer4.Models;

namespace Domain.Security
{
	public static class ApiScopes
	{
		public static ApiScope[] AllScopes => new[] { _nilScope, _mainApiScope };

		public static ApiScope MainAPI => _mainApiScope;

		private static readonly ApiScope _nilScope = new ApiScope("nil", "Empty scope not attached to any API");
		private static readonly ApiScope _mainApiScope = new ApiScope("api", "Main API");
	}
}
