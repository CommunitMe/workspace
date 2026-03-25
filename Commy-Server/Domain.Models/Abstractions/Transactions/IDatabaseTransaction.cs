using Domain.Abstractions.Common;
using Domain.Models.Abstractions.Repositories;

namespace Domain.Models.Abstractions.Transactions
{
    public interface IDatabaseTransaction : ITransaction
    {
        public ICommunityRepository Communities { get; }
        public IProfileRepository Profiles { get; }
        public ICommunityProfileRepository CommunityProfiles { get; }
        public ICommunitySettingsRepository CommunitySettings { get; }
        public IServiceProviderRepository ServiceProviders { get; }
        public IServiceTypeRepository ServiceTypes { get; }
        public IEventRepository Events { get; }
        public IMarketItemRepository MarketItems { get; }
        public ITextTagRepository TextTags { get; }
        public ICurrencyRepository Currencies { get; }
        public ICouponRepository Coupons { get; }
        public INotificationRepository Notifications { get; }
        public ICategoryRepository Categories { get; }
        public ILanguageRepository Languages { get; }
        public IInvitationRepository Invitations { get; }
    }
}
