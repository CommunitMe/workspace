using Domain.Abstractions.Common;
using Domain.Models.Criteria;

namespace Domain.Models
{
	public class TextTag : IPersistantModel<TextTag>
	{
		public long Id { get; set; }
		public string Text { get; set; } = string.Empty;

		public ICriterion<TextTag> GetCriterion()
		{
			return new TextTagCriterion { Id = Id };
		}
	}
}
