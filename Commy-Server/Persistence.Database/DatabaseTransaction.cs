using Domain.Models.Abstractions.Repositories;
using Domain.Models.Abstractions.Transactions;
using Persistence.Database.Repositories;

namespace Persistence.Database
{
    public class DatabaseTransaction : IDatabaseTransaction
    {
        private CmeDbContext _dbContext;

        public CmeDbContext Context { get { return _dbContext; } }


        public ICommunityRepository Communities { get; private set; }
        public IProfileRepository Profiles { get; private set; }
        public ICommunityProfileRepository CommunityProfiles { get; private set; }
        public ICommunitySettingsRepository CommunitySettings { get; private set; }
        public IServiceProviderRepository ServiceProviders { get; private set; }
        public IServiceTypeRepository ServiceTypes { get; private set; }
        public IEventRepository Events { get; private set; }
        public IMarketItemRepository MarketItems { get; private set; }
        public ITextTagRepository TextTags { get; private set; }
        public ICurrencyRepository Currencies { get; private set; }
        public ICouponRepository Coupons { get; private set; }
        public INotificationRepository Notifications { get; private set; }
        public ICategoryRepository Categories { get; private set; }
        public ILanguageRepository Languages { get; private set; }
        public IInvitationRepository Invitations { get; private set; }

        public DatabaseTransaction(CmeDbContext dbContext)
        {
            this._dbContext = dbContext;

            Communities = new CommunityRepository(dbContext);
            Profiles = new ProfileRepository(dbContext);
            CommunityProfiles = new CommunityProfileRepository(dbContext);
            CommunitySettings = new CommunitySettingsRepository(dbContext);
            ServiceProviders = new ServiceProviderRepository(dbContext);
            ServiceTypes = new ServiceTypeRepository(dbContext);
            Events = new EventRepository(dbContext);
            MarketItems = new MarketItemRepository(dbContext);
            TextTags = new TextTagRepository(dbContext);
            Currencies = new CurrencyRepository(dbContext);
            Coupons = new CouponRepository(dbContext);
            Notifications = new NotificationRepository(dbContext);
            Categories = new CategoryRepository(dbContext);
            Languages = new LanguageRepository(dbContext);
            Invitations = new InvitationRepository(dbContext);
        }

        public async Task SaveChangesAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
