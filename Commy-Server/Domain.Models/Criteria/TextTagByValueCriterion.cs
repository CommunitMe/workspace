using Domain.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Criteria
{
	public class TextTagByValueCriterion : ICriterion<TextTag>
	{
		public string Value { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Value
			};
		}

		public bool IsMet(TextTag entity)
		{
			return entity.Text == Value;
		}
	}
}
