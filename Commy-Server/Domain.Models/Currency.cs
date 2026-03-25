using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class Currency : IPersistantModel<Currency>
	{
		public short Id { get; set; }
		public string Code { get; set; } = string.Empty;

		public ICriterion<Currency> GetCriterion()
		{
			return new CurrencyCriterion { Id = Id };
		}
	}
}
