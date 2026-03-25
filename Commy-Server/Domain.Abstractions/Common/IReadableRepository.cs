namespace Domain.Abstractions.Common
{
	public interface IReadableRepository<T>
	{
		Task<List<T>> GetAll(ICriterion<T>? criterion = null);
		Task<T?> GetById(ICriterion<T> criterion);
	}
}
