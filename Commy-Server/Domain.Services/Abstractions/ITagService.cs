using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Abstractions
{
	public interface ITagService
	{
		Task<TextTag> GetOrCreateTextTag(string tag);
		Task<IEnumerable<TextTag>> GetOrCreateTextTags(IEnumerable<string> tags);
	}
}
