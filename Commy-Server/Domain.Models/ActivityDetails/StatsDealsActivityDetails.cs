namespace Domain.Models.ActivityDetails
{
	public class StatsDealsActivityDetails : ActivityDetailsBase
	{
		public Profile Profile { get; set; } = null!;
		public MarketItem MarketItem { get; set; } = null!;
	}
}
