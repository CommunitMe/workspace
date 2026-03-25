namespace Domain.Abstractions.Common
{
	public interface ICriterion<T>
	{
		public bool IsMet(T entity);
		public object[] GetKeys();
	}
}
