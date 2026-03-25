using Mapster;
using Serilog;
using Serilog.Core;

namespace Persistence.Database
{
	public class MapsterRegister : IRegister
	{
		public void Register(TypeAdapterConfig config)
		{
			config.ForType<Entities.Event, Domain.Models.Event>()
				  .Map(dest => dest.Interested,
						src => src.Profiles.Select(p => p.Id))
				  .TwoWays();

			config.ForType<Entities.MarketItem, Domain.Models.MarketItem>()
				.TwoWays()
				.Map(dest => dest.Community,
					 src => src.RelevantCommunity);
		}
	}
}
