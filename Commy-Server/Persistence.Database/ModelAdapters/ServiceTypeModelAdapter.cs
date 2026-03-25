namespace Persistence.Database.ModelAdapters
{
	public static class ServiceTypeModelAdapter
	{
		public static IEnumerable<Domain.Models.ServiceType> ToModel(this IQueryable<Entities.ServiceType> query)
		{
			return query.Select(m => m.ToModel());
		}

		public static Domain.Models.ServiceType ToModel(this Entities.ServiceType model)
		{
			return model.ToModel(null);
		}
		public static Domain.Models.ServiceType ToModel(this Entities.ServiceType model, CmeDbContext? context = null)
		{
			return new Domain.Models.ServiceType
			{
				Id = model.Id,
				Name = model.Name,
			};
		}
	}
}
