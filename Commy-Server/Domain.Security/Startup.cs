using IdentityServer4;
using IdentityServer4.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace Domain.Security
{
	public static class Startup
	{
		public static IServiceCollection ConfigureDomainSecurity(this IServiceCollection services, IConfiguration configuration)
		{
			SecuritySettings? securitySettings = configuration.GetRequiredSection(SecuritySettings.Key).Get<SecuritySettings>();
			if (securitySettings == null)
				throw new Exception("Missing security configuration from settings json");

			var builder = services.AddIdentityServer(options =>
				{
					options.UserInteraction = new UserInteractionOptions()
					{
						LogoutUrl = "/account/logout",
						LoginUrl = "/account/login",
						LoginReturnUrlParameter = "returnUrl"
					};
					options.Events.RaiseFailureEvents = true;
					options.Events.RaiseErrorEvents = true;
					options.Events.RaiseInformationEvents = true;
					options.Events.RaiseSuccessEvents = true;
				})
				.AddDeveloperSigningCredential()
				.AddInMemoryIdentityResources(IdentityResources.AllResources)
				.AddInMemoryApiScopes(ApiScopes.AllScopes)
				.AddInMemoryClients(Clients.AllClients);

			services.AddAuthentication().AddJwtBearer("Bearer", options =>
				{
					options.Authority = securitySettings.JWTAuthority;
					options.TokenValidationParameters = new TokenValidationParameters
					{
						ValidateAudience = false
					};
				});

			services.AddAuthorization(options =>
			{
				options.AddPolicy(IdentityServerConstants.LocalApi.PolicyName, policy =>
				{
					policy.AddAuthenticationSchemes("Bearer");
					policy.RequireAuthenticatedUser();
					policy.RequireClaim("scope", "api");
				});
			});

			services.AddCors(options =>
			{
				options.AddPolicy("default-policy", policy =>
				{
					Log.Logger.Information("Registering CORS policy '{0}' with origins: {1}", "default-policy", securitySettings.CORSOrigins);
					policy.WithOrigins(securitySettings.CORSOrigins)
						  .AllowAnyHeader()
						  .AllowAnyMethod()
						  .AllowCredentials();
				});
			});

			return services;
		}
	}
}