using Domain.Abstractions.Common;
using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Database.Repositories
{
	public class TextTagRepository : ITextTagRepository
	{
		private readonly CmeDbContext _db;

		public TextTagRepository(CmeDbContext dbContext)
		{
			_db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
		}

		public async Task<TextTag> Create(TextTag model)
		{
			var data = model.Adapt<Entities.Tag>();

			var result = _db.Tags.Add(data);
			await _db.SaveChangesAsync();

			return result.Entity.Adapt<Domain.Models.TextTag>();
		}

		public async Task Delete(TextTag model)
		{
			ICriterion<TextTag>? criterion = model.GetCriterion();
			var row = await _db.Tags.FindAsync(criterion.GetKeys());

			if (row == null)
				throw new NullReferenceException($"row of '{nameof(TextTag)}' by criterion '{criterion}' does not exist in DatabaseContext");

			_db.Tags.Remove(row);
			await _db.SaveChangesAsync();
		}

		public Task<List<TextTag>> GetAll(ICriterion<TextTag>? criterion = null)
		{
			var query = _db.Tags.Select(m => m.Adapt<Domain.Models.TextTag>()).AsEnumerable();

			if (criterion != null)
			{
				query = query.Where(row => criterion.IsMet(row));
			}

			return Task.FromResult(query.ToList());
		}

		public async Task<TextTag?> GetById(ICriterion<TextTag> criterion)
		{
			var result = await _db.Tags.FindAsync(criterion.GetKeys());

			if (result == null)
				return null;

			return result.Adapt<Domain.Models.TextTag>();
		}

		public async Task<TextTag> Update(TextTag model)
		{
			var dbEntity = await _db.Tags.FindAsync(model.GetCriterion().GetKeys());

			if (dbEntity == null)
				throw new KeyNotFoundException($"Failed to find {nameof(TextTag)} with given criterion");

			model.Adapt<Entities.Tag>().Adapt(dbEntity);
			await _db.SaveChangesAsync();

			return dbEntity.Adapt<Domain.Models.TextTag>();
		}
	}
}
