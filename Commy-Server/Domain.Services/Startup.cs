using Domain.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Database;

namespace Domain.Services
{
    public static class Startup
    {
        public static IServiceCollection ConfigureDomainServices(this IServiceCollection services)
        {
            services.ConfigureDataServices();

            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<ICommunityService, CommunityService>();
            services.AddScoped<ICommunityProfileService, CommunityProfileService>();
            services.AddScoped<ICommunitySettingsService, CommunitySettingsService>();
            services.AddScoped<ICouponService, CouponService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IImageService, ImageService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IMarketItemService, MarketItemService>();
            services.AddScoped<IProfilesService, ProfilesService>();
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ICurrencyService, CurrenciesService>();
            services.AddScoped<ILanguageService, LanguagesService>();
            services.AddScoped<IServiceProviderService, ServiceProviderService>();
            services.AddScoped<IServiceTypeService, ServiceTypeService>();
            services.AddScoped<IInvitationService, InvitationService>();
            services.AddScoped<ISearchService, SearchService>();
            services.AddScoped<ISmsManager, SmsManager>();

            var searchResultProviders = new Type[]
            {
                typeof(ServiceProviderService),
                typeof(CouponService),
                typeof(MarketItemService),
                typeof(ProfilesService),
                typeof(CategoryService)
            };
            foreach (var resultProvider in searchResultProviders)
            {
                services.Add(new ServiceDescriptor(typeof(ISearchProvider), resultProvider, ServiceLifetime.Transient));
            }

            return services;
        }
    }
}
