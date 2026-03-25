using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Persistence.Database.Entities;

namespace Persistence.Database;

public partial class CmeDbContext : DbContext
{
    public CmeDbContext(DbContextOptions<CmeDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Activity> Activities { get; set; }

    public virtual DbSet<ActivityType> ActivityTypes { get; set; }

    public virtual DbSet<CategoryTree> CategoryTrees { get; set; }

    public virtual DbSet<Community> Communities { get; set; }

    public virtual DbSet<CommunityBalance> CommunityBalances { get; set; }

    public virtual DbSet<CommunityManagerShare> CommunityManagerShares { get; set; }

    public virtual DbSet<CommunityProfile> CommunityProfiles { get; set; }

    public virtual DbSet<CommunitySetting> CommunitySettings { get; set; }

    public virtual DbSet<CommunityShare> CommunityShares { get; set; }

    public virtual DbSet<Coupon> Coupons { get; set; }

    public virtual DbSet<Currency> Currencies { get; set; }

    public virtual DbSet<Event> Events { get; set; }

    public virtual DbSet<Invitation> Invitations { get; set; }

    public virtual DbSet<InvitationsLink> InvitationsLinks { get; set; }

    public virtual DbSet<Language> Languages { get; set; }

    public virtual DbSet<MarketItem> MarketItems { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<OurFlow> OurFlows { get; set; }

    public virtual DbSet<OurShare> OurShares { get; set; }

    public virtual DbSet<PaymentMethod> PaymentMethods { get; set; }

    public virtual DbSet<Profile> Profiles { get; set; }

    public virtual DbSet<ServiceDuration> ServiceDurations { get; set; }

    public virtual DbSet<ServiceProvider> ServiceProviders { get; set; }

    public virtual DbSet<ServiceType> ServiceTypes { get; set; }

    public virtual DbSet<ShareType> ShareTypes { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<TransactionShare> TransactionShares { get; set; }

    public virtual DbSet<UserBalance> UserBalances { get; set; }

    public virtual DbSet<UserShare> UserShares { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Activity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activiti__3213E83FC8793542");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Community).HasColumnName("community");
            entity.Property(e => e.InsertTime)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("insert_time");
            entity.Property(e => e.MarketItem).HasColumnName("market_item");
            entity.Property(e => e.Profile).HasColumnName("profile");
            entity.Property(e => e.ServiceDuration).HasColumnName("service_duration");
            entity.Property(e => e.ServiceType).HasColumnName("service_type");
            entity.Property(e => e.Type).HasColumnName("type");

            entity.HasOne(d => d.CommunityNavigation).WithMany(p => p.Activities)
                .HasForeignKey(d => d.Community)
                .HasConstraintName("FK_Activities_Communities");

            entity.HasOne(d => d.MarketItemNavigation).WithMany(p => p.Activities)
                .HasForeignKey(d => d.MarketItem)
                .HasConstraintName("FK_Activities_Market_Items");

            entity.HasOne(d => d.ProfileNavigation).WithMany(p => p.Activities)
                .HasForeignKey(d => d.Profile)
                .HasConstraintName("FK_Activities_Profiles");

            entity.HasOne(d => d.ServiceDurationNavigation).WithMany(p => p.Activities)
                .HasForeignKey(d => d.ServiceDuration)
                .HasConstraintName("FK_Activities_Service_Duration");

            entity.HasOne(d => d.ServiceTypeNavigation).WithMany(p => p.Activities)
                .HasForeignKey(d => d.ServiceType)
                .HasConstraintName("FK_Activities_Service_Type");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.Activities)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Activities_Activity_Type");
        });

        modelBuilder.Entity<ActivityType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Activity__3213E83F6DDCF4C2");

            entity.ToTable("Activity_Type");

            entity.HasIndex(e => e.Name, "UQ__Activity__72E12F1B54680B7D").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CategoryTree>(entity =>
        {
            entity.ToTable("Category_Tree");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Parent).HasColumnName("parent");

            entity.HasOne(d => d.ParentNavigation).WithMany(p => p.InverseParentNavigation)
                .HasForeignKey(d => d.Parent)
                .HasConstraintName("FK_Category_Tree_To_Category_Tree");
        });

        modelBuilder.Entity<Community>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Communit__3213E83F8AC003EE");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .HasDefaultValueSql("('')")
                .HasColumnName("description");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
        });

        modelBuilder.Entity<CommunityBalance>(entity =>
        {
            entity.ToTable("Community_Balance", "finance_data");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.CurrentBalance)
                .HasColumnType("money")
                .HasColumnName("current_balance");

            entity.HasOne(d => d.Community).WithMany(p => p.CommunityBalances)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Community_Balance_To_Communities");
        });

        modelBuilder.Entity<CommunityManagerShare>(entity =>
        {
            entity.ToTable("Community_Manager_Share", "finance_settings");

            entity.HasIndex(e => new { e.Id, e.Type, e.Value }, "UC_Community_Manager_Share").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("value");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.CommunityManagerShares)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Community_Manager_Share_To_Share_Type");
        });

        modelBuilder.Entity<CommunityProfile>(entity =>
        {
            entity.HasKey(e => new { e.CommunityId, e.ProfileId });

            entity.ToTable("Community_Profiles");

            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.MemberState).HasColumnName("member_state");
            entity.Property(e => e.ProviderState).HasColumnName("provider_state");

            entity.HasOne(d => d.Community).WithMany(p => p.CommunityProfiles)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Community_Profiles_Communities");

            entity.HasOne(d => d.Profile).WithMany(p => p.CommunityProfiles)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Community_Profiles_Profiles");
        });

        modelBuilder.Entity<CommunitySetting>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Community_Settings__3213E83F2E1F4DE7");

            entity.ToTable("Community_Settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AgeRestriction).HasColumnName("age_restriction");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.CommunityUsername)
                .HasMaxLength(255)
                .HasColumnName("community_username");
            entity.Property(e => e.DefaultCurrency)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("default_currency");
            entity.Property(e => e.DefaultLanguage)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("default_language");
            entity.Property(e => e.Guidelines).HasColumnName("guidelines");
            entity.Property(e => e.MembersApprovalPreferences).HasColumnName("members_approval_preferences");
            entity.Property(e => e.Privacy).HasColumnName("privacy");
            entity.Property(e => e.SuppliersApprovalPreferences).HasColumnName("suppliers_approval_preferences");

            entity.HasOne(d => d.Community).WithMany(p => p.CommunitySettings)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Community_Settings_Communities");
        });

        modelBuilder.Entity<CommunityShare>(entity =>
        {
            entity.ToTable("Community_Share", "finance_settings");

            entity.HasIndex(e => new { e.Id, e.Type, e.Value }, "UC_Community_Share").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("value");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.CommunityShares)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Community_Share_To_Share_Type");
        });

        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Coupons__3213E83F2E1F4DE7");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Expiration)
                .HasColumnType("datetime")
                .HasColumnName("expiration");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.IsActive).HasColumnName("isActive");
            entity.Property(e => e.LocationName)
                .HasMaxLength(255)
                .HasColumnName("locationName");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.RelevantCommunity).HasColumnName("relevant_community");
            entity.Property(e => e.ServiceProviderId).HasColumnName("service_provider_id");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Coupons_Category_Tree");

            entity.HasOne(d => d.RelevantCommunityNavigation).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.RelevantCommunity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Coupons_Communities");

            entity.HasOne(d => d.ServiceProvider).WithMany(p => p.Coupons)
                .HasForeignKey(d => d.ServiceProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Coupons_Service_Providers");

            entity.HasMany(d => d.Tags).WithMany(p => p.Coupons)
                .UsingEntity<Dictionary<string, object>>(
                    "CouponsTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Coupon_Tags_Tags"),
                    l => l.HasOne<Coupon>().WithMany()
                        .HasForeignKey("CouponId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Coupon_Tags_Coupon"),
                    j =>
                    {
                        j.HasKey("CouponId", "TagId").HasName("PK_Coupon_Tags");
                        j.ToTable("Coupons_Tags");
                        j.IndexerProperty<long>("CouponId").HasColumnName("coupon_id");
                        j.IndexerProperty<long>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Currency>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Currenci__3213E83FA6106A9C");

            entity.ToTable(tb => tb.HasComment("Based of ISO-4217"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("code");
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Events__3213E83F727FEFA9");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.EventTime)
                .HasColumnType("datetime")
                .HasColumnName("event_time");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .HasColumnName("location");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.RelevantCommunity).HasColumnName("relevant_community");

            entity.HasOne(d => d.RelevantCommunityNavigation).WithMany(p => p.Events)
                .HasForeignKey(d => d.RelevantCommunity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Events_Communities");

            entity.HasMany(d => d.Profiles).WithMany(p => p.Events)
                .UsingEntity<Dictionary<string, object>>(
                    "EventsProfile",
                    r => r.HasOne<Profile>().WithMany()
                        .HasForeignKey("ProfileId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Events_Profiles_Profiles"),
                    l => l.HasOne<Event>().WithMany()
                        .HasForeignKey("EventId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Events_Profiles_Events"),
                    j =>
                    {
                        j.HasKey("EventId", "ProfileId").HasName("PK__Events_P__C99B4026202C7A38");
                        j.ToTable("Events_Profiles");
                        j.IndexerProperty<long>("EventId").HasColumnName("event_id");
                        j.IndexerProperty<long>("ProfileId").HasColumnName("profile_id");
                    });
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.HasKey(e => e.Id).IsClustered(false);

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.AllowedUsages).HasColumnName("allowed_usages");
            entity.Property(e => e.ClusteredIndex)
                .ValueGeneratedOnAdd()
                .HasColumnName("clustered_index");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.CreationTime)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("creation_time");
            entity.Property(e => e.CreatorId).HasColumnName("creator_id");
            entity.Property(e => e.Expiry)
                .HasColumnType("datetime")
                .HasColumnName("expiry");
            entity.Property(e => e.InvitationType).HasColumnName("invitation_type");

            entity.HasOne(d => d.Community).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitations_Communities");

            entity.HasOne(d => d.Creator).WithMany(p => p.Invitations)
                .HasForeignKey(d => d.CreatorId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitations_Profiles");
        });

        modelBuilder.Entity<InvitationsLink>(entity =>
        {
            entity.ToTable("Invitations_Links");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.InvitationId).HasColumnName("invitation_id");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamp");
            entity.Property(e => e.Usages).HasColumnName("usages");

            entity.HasOne(d => d.Invitation).WithMany(p => p.InvitationsLinks)
                .HasForeignKey(d => d.InvitationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Invitations_Links_Invitations");
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Language__3213E83FA6106A9C");

            entity.ToTable(tb => tb.HasComment("Based of ISO-4217"));

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(3)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<MarketItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Market_I__3213E83F967293C5");

            entity.ToTable("Market_Items");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Description)
                .HasMaxLength(1024)
                .HasColumnName("description");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.Owner).HasColumnName("owner");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.PriceCurrency).HasColumnName("price_currency");
            entity.Property(e => e.RelevantCommunity).HasColumnName("relevant_community");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.MarketItems)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Market_Items_Category_Tree");

            entity.HasOne(d => d.OwnerNavigation).WithMany(p => p.MarketItems)
                .HasForeignKey(d => d.Owner)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Market_Items_Profiles");

            entity.HasOne(d => d.PriceCurrencyNavigation).WithMany(p => p.MarketItems)
                .HasForeignKey(d => d.PriceCurrency)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Market_Items_Currencies");

            entity.HasOne(d => d.RelevantCommunityNavigation).WithMany(p => p.MarketItems)
                .HasForeignKey(d => d.RelevantCommunity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Market_Items_Communities");

            entity.HasMany(d => d.Tags).WithMany(p => p.Items)
                .UsingEntity<Dictionary<string, object>>(
                    "MarketItemsTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Market_Item_Tags_Tags"),
                    l => l.HasOne<MarketItem>().WithMany()
                        .HasForeignKey("ItemId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Market_Item_Tags_Market_Item"),
                    j =>
                    {
                        j.HasKey("ItemId", "TagId").HasName("PK_Market_Item_Tags");
                        j.ToTable("Market_Items_Tags");
                        j.IndexerProperty<long>("ItemId").HasColumnName("item_id");
                        j.IndexerProperty<long>("TagId").HasColumnName("tag_id");
                    });
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Notifica__3213E83F47B3797E");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(256)
                .HasColumnName("description");
            entity.Property(e => e.InsertTime)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime")
                .HasColumnName("insert_time");
            entity.Property(e => e.RelevantCommunity).HasColumnName("relevant_community");
            entity.Property(e => e.Title)
                .HasMaxLength(256)
                .HasColumnName("title");

            entity.HasOne(d => d.RelevantCommunityNavigation).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.RelevantCommunity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Notifications_Communities");
        });

        modelBuilder.Entity<OurFlow>(entity =>
        {
            entity.ToTable("Our_Flow", "finance_data");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("money")
                .HasColumnName("amount");
            entity.Property(e => e.Timestamp)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestamp");
            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");

            entity.HasOne(d => d.Transaction).WithMany(p => p.OurFlows)
                .HasForeignKey(d => d.TransactionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Our_Flow_To_Transactions");
        });

        modelBuilder.Entity<OurShare>(entity =>
        {
            entity.ToTable("Our_Share", "finance_settings");

            entity.HasIndex(e => new { e.Id, e.Type, e.Value }, "UC_Our_Share").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("value");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.OurShares)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Our_Share_To_Share_Type");
        });

        modelBuilder.Entity<PaymentMethod>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Payment_Method");

            entity.ToTable("Payment_Methods", "finance_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PaymentMethod1)
                .HasMaxLength(50)
                .HasColumnName("payment_method");
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__AEBB701F9411C072");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessFailedCount).HasColumnName("access_failed_count");
            entity.Property(e => e.Address)
                .HasMaxLength(256)
                .HasColumnName("address");
            entity.Property(e => e.Birthday)
                .HasColumnType("date")
                .HasColumnName("birthday");
            entity.Property(e => e.City)
                .HasMaxLength(256)
                .HasColumnName("city");
            entity.Property(e => e.ConcurrencyStamp)
                .HasMaxLength(256)
                .HasColumnName("concurrency_stamp");
            entity.Property(e => e.DefaultCommunity).HasColumnName("default_community");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.EmailConfirmed).HasColumnName("email_verified");
            entity.Property(e => e.FamilyName)
                .HasMaxLength(256)
                .HasColumnName("family_name");
            entity.Property(e => e.GivenName)
                .HasMaxLength(256)
                .HasColumnName("given_name");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .HasColumnName("last_login");
            entity.Property(e => e.LastOpenNotifications)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("last_open_notifications");
            entity.Property(e => e.LockoutEnabled)
                .IsRequired()
                .HasDefaultValueSql("((1))")
                .HasColumnName("lockout_enabled");
            entity.Property(e => e.LockoutEnd)
                .HasConversion(
                    v => v.HasValue ? v.Value.UtcDateTime : (DateTime?)null,
                    v => v.HasValue ? new DateTimeOffset(v.Value) : null)
                .HasColumnType("datetime")
                .HasColumnName("lockout_end");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(256)
                .HasColumnName("middle_name");
            entity.Property(e => e.NormalizedEmail)
                .HasMaxLength(256)
                .HasColumnName("normalized_email");
            entity.Property(e => e.NormalizedUserName)
                .HasMaxLength(256)
                .HasColumnName("normalized_user_name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(256)
                .HasColumnName("password_hash");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(256)
                .HasColumnName("phone");
            entity.Property(e => e.PhoneNumberConfirmed).HasColumnName("phone_verified");
            entity.Property(e => e.PostalCode)
                .HasMaxLength(256)
                .HasColumnName("postal_code");
            entity.Property(e => e.SecurityStamp)
                .HasMaxLength(256)
                .HasColumnName("security_stamp");
            entity.Property(e => e.ServiceProvider).HasColumnName("service_provider");
            entity.Property(e => e.State)
                .HasMaxLength(256)
                .HasColumnName("state");
            entity.Property(e => e.TwoFactorEnabled).HasColumnName("two_factor_enabled");
            entity.Property(e => e.UserName)
                .HasMaxLength(256)
                .HasColumnName("user_name");

            entity.HasOne(d => d.DefaultCommunityNavigation).WithMany(p => p.Profiles)
                .HasForeignKey(d => d.DefaultCommunity)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Profiles_Communities");

            entity.HasOne(d => d.ServiceProviderNavigation).WithMany(p => p.Profiles)
                .HasForeignKey(d => d.ServiceProvider)
                .HasConstraintName("FK_Profiles_Service_Providers");
        });

        modelBuilder.Entity<ServiceDuration>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Service___3213E83FF49006C3");

            entity.ToTable("Service_Duration");

            entity.HasIndex(e => e.Name, "UQ__Service___72E12F1B719D39C8").IsUnique();

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<ServiceProvider>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__tmp_ms_x__3213E83F37E9295C");

            entity.ToTable("Service_Providers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email)
                .HasMaxLength(256)
                .HasColumnName("email");
            entity.Property(e => e.ImageUid)
                .HasMaxLength(50)
                .HasColumnName("image_uid");
            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(256)
                .HasColumnName("phone");
            entity.Property(e => e.ServiceProviderType)
                .HasDefaultValueSql("((1))")
                .HasColumnName("service_provider_type");

            entity.HasMany(d => d.ServiceTypes).WithMany(p => p.ServiceProviders)
                .UsingEntity<Dictionary<string, object>>(
                    "ServiceProvidersServiceType",
                    r => r.HasOne<ServiceType>().WithMany()
                        .HasForeignKey("ServiceType")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Service_Providers_Service_Type_Service_Type"),
                    l => l.HasOne<ServiceProvider>().WithMany()
                        .HasForeignKey("ServiceProviderId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_Service_Providers_Service_Type_Service_Providers"),
                    j =>
                    {
                        j.HasKey("ServiceProviderId", "ServiceType").HasName("PK__Service_Provider__2CACD5E6D63DD648");
                        j.ToTable("Service_Providers_Service_Type");
                        j.IndexerProperty<long>("ServiceProviderId").HasColumnName("service_provider_id");
                        j.IndexerProperty<long>("ServiceType").HasColumnName("service_type");
                    });
        });

        modelBuilder.Entity<ServiceType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Service___3213E83F60C9C2BE");

            entity.ToTable("Service_Type");

            entity.HasIndex(e => e.Name, "UQ__Service___72E12F1B70872160").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            entity.HasOne(d => d.CategoryNavigation).WithMany(p => p.ServiceTypes)
                .HasForeignKey(d => d.Category)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Service_Type_To_Category_Tree");
        });

        modelBuilder.Entity<ShareType>(entity =>
        {
            entity.ToTable("Share_Type", "finance_settings");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3213E83F1C399C8C");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Text)
                .HasMaxLength(256)
                .HasColumnName("text");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.ToTable("Transactions", "finance_data");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CommunityId).HasColumnName("community_id");
            entity.Property(e => e.CommunityShare)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("community_share");
            entity.Property(e => e.Currency)
                .HasMaxLength(50)
                .HasColumnName("currency");
            entity.Property(e => e.OurShare)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("our_share");
            entity.Property(e => e.PaymentMethod).HasColumnName("payment_method");
            entity.Property(e => e.Price)
                .HasColumnType("money")
                .HasColumnName("price");
            entity.Property(e => e.ProductId).HasColumnName("product_id");
            entity.Property(e => e.ProfileId).HasColumnName("profile_id");
            entity.Property(e => e.ServiceProviderShare)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("service_provider_share");
            entity.Property(e => e.Timestampt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("timestampt");
            entity.Property(e => e.UserShare)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("user_share");

            entity.HasOne(d => d.Community).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_To_Communities");

            entity.HasOne(d => d.PaymentMethodNavigation).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PaymentMethod)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_To_Payment_Methods");

            entity.HasOne(d => d.Profile).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_To_Profiles");
        });

        modelBuilder.Entity<TransactionShare>(entity =>
        {
            entity.HasKey(e => e.CommunityId);

            entity.ToTable("Transaction_Shares", "finance_settings");

            entity.Property(e => e.CommunityId)
                .ValueGeneratedNever()
                .HasColumnName("community_id");
            entity.Property(e => e.CommunityManagerShareId).HasColumnName("community_manager_share_id");
            entity.Property(e => e.CommunityShareId).HasColumnName("community_share_id");
            entity.Property(e => e.OurShareId).HasColumnName("our_share_id");
            entity.Property(e => e.UserShareId).HasColumnName("user_share_id");

            entity.HasOne(d => d.Community).WithOne(p => p.TransactionShare)
                .HasForeignKey<TransactionShare>(d => d.CommunityId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transaction_Shares_To_Communities");

            entity.HasOne(d => d.CommunityManagerShare).WithMany(p => p.TransactionShares)
                .HasForeignKey(d => d.CommunityManagerShareId)
                .HasConstraintName("FK_Transaction_Shares_To_Community_Manager_Share");

            entity.HasOne(d => d.CommunityShare).WithMany(p => p.TransactionShares)
                .HasForeignKey(d => d.CommunityShareId)
                .HasConstraintName("FK_Transaction_Shares_To_Community_Share");

            entity.HasOne(d => d.OurShare).WithMany(p => p.TransactionShares)
                .HasForeignKey(d => d.OurShareId)
                .HasConstraintName("FK_Transaction_Shares_To_Commy_Share");

            entity.HasOne(d => d.UserShare).WithMany(p => p.TransactionShares)
                .HasForeignKey(d => d.UserShareId)
                .HasConstraintName("FK_Transaction_Shares_To_User_Share");
        });

        modelBuilder.Entity<UserBalance>(entity =>
        {
            entity.HasKey(e => e.ProfileId);

            entity.ToTable("User_Balance", "finance_data");

            entity.Property(e => e.ProfileId)
                .ValueGeneratedNever()
                .HasColumnName("profile_id");
            entity.Property(e => e.CurrentBalance)
                .HasColumnType("money")
                .HasColumnName("current_balance");

            entity.HasOne(d => d.Profile).WithOne(p => p.UserBalance)
                .HasForeignKey<UserBalance>(d => d.ProfileId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Balance_To_Profiles");
        });

        modelBuilder.Entity<UserShare>(entity =>
        {
            entity.ToTable("User_Share", "finance_settings");

            entity.HasIndex(e => new { e.Id, e.Type, e.Value }, "UC_User_Share").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description)
                .HasMaxLength(250)
                .HasColumnName("description");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Value)
                .HasColumnType("decimal(3, 2)")
                .HasColumnName("value");

            entity.HasOne(d => d.TypeNavigation).WithMany(p => p.UserShares)
                .HasForeignKey(d => d.Type)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_User_Share_To_Share_Type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
