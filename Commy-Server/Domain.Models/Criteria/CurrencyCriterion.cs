using Domain.Abstractions.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models.Criteria
{
	public class CurrencyCriterion : ICriterion<Currency>
	{
		public long Id { get; set; }

		public object[] GetKeys()
		{
			return new object[]
			{
				Id
			};
		}

		public bool IsMet(Currency entity)
		{
			return entity.Id == Id;
		}
	}
}
