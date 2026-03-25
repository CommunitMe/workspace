using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Infra.Exceptions
{
	public class PersistantItemNotFoundException : Exception
	{
		public Type ItemType { get; private set; }

		public PersistantItemNotFoundException(Type itemType)
			: base($"Item of type '{itemType.Name}' was expected to exist, but was not found in persistance layer.")
		{
			ItemType = itemType;
		}
	}
}
