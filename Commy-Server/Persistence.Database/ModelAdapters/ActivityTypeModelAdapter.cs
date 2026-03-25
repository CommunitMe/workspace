using Domain.Models.Enums;

namespace Persistence.Database.ModelAdapters
{
	public static class ActivityTypeModelAdapter
	{
		public static IEnumerable<ActivityType> ToModel(this IQueryable<Entities.ActivityType> query)
		{
			return query.Select(m => m.ToModel());
		}

		public static ActivityType ToModel(this Entities.ActivityType model)
		{
			return model.ToModel(null);
		}
		public static ActivityType ToModel(this Entities.ActivityType model, CmeDbContext? context = null)
		{
			return ActivityType.Parse(model.Name);
		}
	}
}
