namespace Persistence.Database.ModelAdapters
{
	public static class ServiceDurationModelAdapter
	{
		public static IEnumerable<Domain.Models.Enums.ServiceDuration> ToModel(this IQueryable<Entities.ServiceDuration> query)
		{
			return query.Select(m => m.ToModel());
		}

		public static Domain.Models.Enums.ServiceDuration ToModel(this Entities.ServiceDuration model)
		{
			return model.ToModel(null);
		}
		public static Domain.Models.Enums.ServiceDuration ToModel(this Entities.ServiceDuration model, CmeDbContext? context = null)
		{
			return Domain.Models.Enums.ServiceDuration.Parse(model.Name);
		}
	}
}
