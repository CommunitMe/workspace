using Domain.Models.Abstractions.Repositories;
using Domain.Models.Abstractions.Transactions;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Database.Entities;
using Persistence.Database.Repositories;
using Serilog;

namespace Persistence.Database
{
	public static class Startup
	{
		public static IServiceCollection ConfigureDataServices(this IServiceCollection services)
		{
			services.AddSingleton<IRegister, MapsterRegister>();

			services.AddDbContext<CmeDbContext>((provider, options) =>
			{
				IConfiguration config = provider.GetRequiredService<IConfiguration>();
				string? connectionString = config.GetConnectionString("DefaultConnection");

				if (connectionString == null)
					throw new NullReferenceException("Failed to get connection string configuration");



				options.EnableSensitiveDataLogging();
				Log.Logger.Verbose("Creating context using connection string: {0}", connectionString);

				options.UseSqlServer(connectionString);
			});

			services.AddIdentityCore<Profile>()
				.AddRoles<IdentityRole<long>>()
				.AddEntityFrameworkStores<CmeDbContext>();

			services.AddTransient<IDatabaseTransaction, DatabaseTransaction>();

			services.AddScoped<IActivityRespository, ActivityRepository>();

			return services;
		}
	}
}
