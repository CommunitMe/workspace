using Domain.Security;
using Domain.Services;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using System.Security.Claims;
using System.Text.Json.Serialization;

namespace WebAPI
{
	public class Program
	{
		public static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Override("Microsoft", LogEventLevel.Information)
				.Enrich.FromLogContext()
				.WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] [Bootstrap] {Message:lj}{NewLine}{Exception}")
				.WriteTo.File("./logs/bootstrap_log.log",
							  rollOnFileSizeLimit: true,
							  retainedFileCountLimit: 1,
							  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
				.CreateBootstrapLogger();

			try
			{
				var builder = WebApplication.CreateBuilder(args);
				Log.Logger.Information("Application composition started");
				SetupBuilder(builder);
				Log.Logger.Information("Application composition complete");
				var app = builder.Build();
				Log.Logger.Information("Application built, setup started");
				SetupApplication(app);
				Log.Logger.Information("Application setup complete");
				app.Run();
				Log.Logger.Information("Application ran to completion, shutting down");
			}
			catch (Exception ex)
			{
				Log.Fatal(ex, "Application terminated unexpectedly");
			}
			finally
			{
				Log.CloseAndFlush();
			}
		}

		private static void SetupBuilder(WebApplicationBuilder builder)
		{
			builder.Host.UseSerilog((context, services, configuration) => configuration
				.MinimumLevel.Override("System", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
				.MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
				.MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
				.ReadFrom.Configuration(context.Configuration)
				.ReadFrom.Services(services)
				.Enrich.WithProperty("User", "System")
				.Enrich.FromLogContext()
				.WriteTo.File("./logs/log_.log",
							  rollingInterval: RollingInterval.Day,
							  rollOnFileSizeLimit: true,
							  retainedFileCountLimit: 31,
							  outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{User}] {Message:lj}{NewLine}{Exception}")
				.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} [{Level:u3}] [{User}] {Message:lj}{NewLine}{Exception}")
			);

			Log.Logger.Information("Application building for env '{0}'", builder.Environment.EnvironmentName);

			// Add services to the container.
			builder.Services.AddControllersWithViews(); // For IS4MVC
			builder.Services.AddControllers()
							.AddJsonOptions(options =>
							{
								options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
							});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen(options =>
			{
				options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
				{
					Description = "OAuth2 Authentication",
					Type = SecuritySchemeType.OAuth2,
					Flows = new OpenApiOAuthFlows
					{
						AuthorizationCode = new OpenApiOAuthFlow
						{
							AuthorizationUrl = new Uri("https://localhost:5001/connect/authorize"),
							TokenUrl = new Uri("https://localhost:5001/connect/token"),
							Scopes = ApiScopes.AllScopes.ToDictionary(scope => scope.Name, scope => scope.Description)
						}
					}
				});

				options.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference =  new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id = "OAuth2"
							}
						},
						new string[]{ }
					}
				});
			});

			builder.Services.ConfigureDomainSecurity(builder.Configuration);
			builder.Services.ConfigureDomainServices();
		}

		private static void SetupApplication(WebApplication app)
		{
			app.ConfigureMapster();

			app.UseSerilogRequestLogging(options =>
			{
				options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
				{
					Claim? userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier);

					if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
					{
						diagnosticContext.Set("User", $"UserID: {userId}");
					}
					else
					{
						diagnosticContext.Set("User", "Anon");
					}
				};
			});

			if (!app.Environment.IsProduction())
			{
				app.UseDeveloperExceptionPage();

				app.UseSwagger();
				app.UseSwaggerUI(options =>
				{
					options.OAuthClientId(Clients.Swagger.ClientId);
					options.OAuthAppName("CommunitMe Client");
					options.OAuthUsePkce();
				});
			}

			app.UseCors("default-policy");

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting(); // For IS4MVC

			app.UseIdentityServer();
			app.UseAuthentication();
			app.UseAuthorization();

			app.MapDefaultControllerRoute().RequireAuthorization();
		}
	}
}
