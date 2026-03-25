namespace Domain.Abstractions.Common
{
	public interface IPersistantModel<T>
	{
		ICriterion<T> GetCriterion();
	}
}
