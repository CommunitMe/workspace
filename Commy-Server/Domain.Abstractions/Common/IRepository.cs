namespace Domain.Abstractions.Common
{
	public interface IRepository<T> : IReadableRepository<T> where T : IPersistantModel<T>
	{
		Task<T> Create(T model);
		Task<T> Update(T model);
		Task Delete(T model);
	}
}
