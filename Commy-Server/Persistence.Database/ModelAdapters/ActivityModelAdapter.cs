using Domain.Models.ActivityDetails;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Database.ModelAdapters
{
	public static class ActivityModelAdapter
	{
		public static IEnumerable<Domain.Models.Activity> ToModel(this IQueryable<Entities.Activity> query)
		{
			return query.Include(m => m.TypeNavigation)
						.Include(m => m.ServiceDurationNavigation)
						.Include(m => m.ServiceTypeNavigation)
						.Select(m => m.ToModel());
		}
		public static Domain.Models.Activity ToModel(this Entities.Activity model)
		{
			return model.ToModel(null);
		}
		public static Domain.Models.Activity ToModel(this Entities.Activity model, CmeDbContext? context = null)
		{
			if (context != null)
			{
				context.Entry(model).Reference(m => m.TypeNavigation).Load();
			}

			var result = new Domain.Models.Activity
			{
				Id = model.Id,
				InsertTime = model.InsertTime,
				Type = model.TypeNavigation.ToModel(context),
				ActivityDetails = GetActivityDetails(model, context)
			};

			return result;
		}

		private static ActivityDetailsBase GetActivityDetails(Entities.Activity model, CmeDbContext? context = null)
		{
			switch (model.TypeNavigation.Name)
			{
				case "LOOKING_FOR_SERVICE": return GetLookingForServiceActivityDetails(model, context);
				case "NEW_SERVICE_PROVIDER": return GetNewServiceProviderActivityDetails(model, context);
				case "SELLING": return GetSellingActivityDetails(model, context);
				default: throw new NotImplementedException($"Mapping for '{model.TypeNavigation.GetType().Name}.{model.TypeNavigation.Name}' is not implemented");
			}
		}

		private static LookingForServiceActivityDetails GetLookingForServiceActivityDetails(Entities.Activity model, CmeDbContext? context = null)
		{
			if (context != null)
			{
				context.Entry(model).Reference(m => m.ServiceDurationNavigation).Load();
				context.Entry(model).Reference(m => m.ServiceTypeNavigation).Load();
			}

			if (!model.Profile.HasValue)
				throw new Exception("Profile ID must be provided");

			var serviceDuration = model.ServiceDurationNavigation;
			if (serviceDuration == null)
				throw new Exception("Failed to get referenced value");

			var serviceType = model.ServiceTypeNavigation;
			if (serviceType == null)
				throw new Exception("Failed to get referenced value");

			return new LookingForServiceActivityDetails
			{
				ProfileId = model.Profile.Value,
				ServiceDuration = serviceDuration.ToModel(context),
				ServiceType = serviceType.ToModel(context)
			};
		}

		private static NewServiceProviderActivityDetails GetNewServiceProviderActivityDetails(Entities.Activity model, CmeDbContext? context = null)
		{
			if (context != null)
			{
				context.Entry(model).Reference(m => m.ServiceTypeNavigation).Load();
			}

			if (!model.Profile.HasValue)
				throw new Exception("Profile ID must be provided");
			if (!model.Community.HasValue)
				throw new Exception("Community ID must be provided");

			var serviceType = model.ServiceTypeNavigation;
			if (serviceType == null)
				throw new Exception("Failed to get referenced value");

			return new NewServiceProviderActivityDetails
			{
				ServiceType = serviceType.ToModel(context),
				CommunityId = model.Community.Value,
				ProfileId = model.Profile.Value
			};
		}

		private static SellingActivityDetails GetSellingActivityDetails(Entities.Activity model, CmeDbContext? context = null)
		{
			if (context != null)
			{
			}

			if (!model.Profile.HasValue)
				throw new Exception("Profile ID must be provided");
			if (!model.MarketItem.HasValue)
				throw new Exception("Market Item ID must be provided");
			return new SellingActivityDetails
			{
				MarketItemId = model.MarketItem.Value,
				ProfileId = model.Profile.Value
			};
		}
	}
}
