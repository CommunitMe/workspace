using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
	public class TagService : ITagService
	{
		private readonly ILogger<ProfilesService> logger;
		private readonly IDatabaseTransaction db;

		public TagService(ILogger<ProfilesService> logger, IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task<IEnumerable<TextTag>> GetOrCreateTextTags(IEnumerable<string> tags)
		{
			List<TextTag> tagsList = new List<TextTag>();

			foreach (string tag in tags)
			{
				TextTag instance = await GetOrCreateTextTag(tag);
				tagsList.Add(instance);
			}

			return tagsList.AsEnumerable();
		}

		public async Task<TextTag> GetOrCreateTextTag(string tag)
		{
			List<TextTag> tags = await db.TextTags.GetAll(new TextTagByValueCriterion { Value = tag });

			if (tags.Count > 1)
			{
				logger.LogWarning("More than one tag exist with the same value!");
				return tags.First();
			}

			if (tags.Count == 1)
			{
				return tags.Single();
			}

			logger.LogInformation("Tag '{0}' does not exist, creating new on", tag);
			TextTag newTag = await db.TextTags.Create(new TextTag { Text = tag });
			await db.SaveChangesAsync();
			return newTag;
		}
	}
}
