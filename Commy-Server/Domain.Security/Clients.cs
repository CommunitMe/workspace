using IdentityServer4;
using IdentityServer4.Models;

namespace Domain.Security
{
	public static class Clients
	{
		public static Client[] AllClients => new[]{
			_swaggerClient,
			_devClient,
			_stagingClient,
			_prodClient
		};

		public static Client Swagger => _swaggerClient;

		private static readonly Client _swaggerClient = CreateClient(client =>
		{
			client.ClientName = "Swagger Client";
			client.ClientId = "dev-swag";
			client.RedirectUris = new List<string> { "https://localhost:5001/swagger/oauth2-redirect.html" };
			client.PostLogoutRedirectUris = new List<string> { "http://localhost:4200/signout-callback" };
			client.AccessTokenLifetime = 600;
			client.AllowedScopes = ApiScopes.AllScopes.Select(scope => scope.Name).ToList();

			return client;
		});

		private static readonly Client _devClient = CreateClient(client =>
		{
			client.ClientName = "Development Client";
			client.ClientId = "dev-client";

			string apiBase = "http://localhost:4200";
			client.RedirectUris = new List<string> { GetSiginRedirectURL(apiBase) };
			client.PostLogoutRedirectUris = new List<string> { GetSignoutRedirectURL(apiBase) };
			client.AllowedCorsOrigins = new List<string> { "http://localhost:4200" };
			client.AccessTokenLifetime = 600;

			return client;
		});

		private static readonly Client _stagingClient = CreateClient(client =>
		{
			client.ClientName = "Staging Client";
			client.ClientId = "staging-client";

			string apiBase = "https://staging.communitme.com";
			client.RedirectUris = new List<string> { GetSiginRedirectURL(apiBase) };
			client.PostLogoutRedirectUris = new List<string> { GetSignoutRedirectURL(apiBase) };
			client.AllowedCorsOrigins = new List<string> { "http://localhost:4200", "https://staging.communitme.com" };
			return client;
		});

		private static readonly Client _prodClient = CreateClient(client =>
		{
			client.ClientName = "Production Client";
			client.ClientId = "prod-client";

			string apiBase = "https://communitme.com";
			client.RedirectUris = new List<string> { GetSiginRedirectURL(apiBase) };
			client.PostLogoutRedirectUris = new List<string> { GetSignoutRedirectURL(apiBase) };
			client.AllowedCorsOrigins = new List<string> { "https://communitme.com" };
			return client;
		});

		private static Client CreateClient(Func<Client, Client> setupCallback)
		{
			Client client = new Client
			{
				AllowedGrantTypes = GrantTypes.Code,
				AllowAccessTokensViaBrowser = true,
				AllowedScopes = {
					IdentityServerConstants.StandardScopes.OpenId,
					IdentityServerConstants.StandardScopes.Profile,
					"api"
				},
				RequireClientSecret = false,
				RequireConsent = false,
				RequirePkce = true,
			};

			return setupCallback(client);
		}

		private static string GetSiginRedirectURL(string apiBase)
		{
			return apiBase + "/signin-callback";
		}
		private static string GetSignoutRedirectURL(string apiBase)
		{
			return apiBase + "/signout-callback";
		}
	}
}
