using Domain.Models;
using Domain.Models.Abstractions.Repositories;
using Domain.Models.Abstractions.Transactions;
using Domain.Models.Criteria;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace Domain.Services
{
	public class EventService : IEventService
	{
		private readonly ILogger<EventService> logger;
		private readonly IDatabaseTransaction db;

		public EventService(ILogger<EventService> logger, IDatabaseTransaction db)
		{
			this.logger = logger;
			this.db = db;
		}

		public async Task DeleteEvent(Event eventData)
		{
			await db.Events.Delete(eventData);
			await db.SaveChangesAsync();
		}

		public async Task<Event> CreateEvent(Event eventData)
		{
			eventData = await db.Events.Create(eventData);
			await db.SaveChangesAsync();
			return eventData;
		}

		public async Task<Event> UpdateEvent(Event eventData)
		{
			eventData = await db.Events.Update(eventData);
			await db.SaveChangesAsync();
			return eventData;
		}

		public async Task<Event?> GetEventById(long eid)
		{
			var eventData = await db.Events.GetById(new EventCriterion { Id = eid });

			if (eventData == null)
			{
				this.logger.LogInformation(message: "No event item with id '{0}'", eid);
			}

			return eventData;
		}

		public async Task<List<Event>?> GetEventByCommunityId(long cid, int? maxAmount = null)
		{
			var filter = new EventByCommunityCriterion { CommunityId = cid };

			IEnumerable<Event> result = await db.Events.GetAll(filter);

			if (maxAmount.HasValue)
			{
				result = result.Take(maxAmount.Value);
			}

			return result.ToList();
		}
	}
}
