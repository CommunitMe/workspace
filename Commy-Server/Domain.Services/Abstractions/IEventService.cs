using Domain.Models;

namespace Domain.Services.Abstractions
{
	public interface IEventService
	{
		Task<Event> CreateEvent(Event eventData);
		Task DeleteEvent(Event eventData);
		Task<List<Event>?> GetEventByCommunityId(long cid, int? maxAmount = null);
		Task<Event?> GetEventById(long eid);
		Task<Event> UpdateEvent(Event eventData);
	}
}
