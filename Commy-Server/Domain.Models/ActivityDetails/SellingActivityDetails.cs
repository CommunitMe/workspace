namespace Domain.Models.ActivityDetails
{
	public class SellingActivityDetails : ActivityDetailsBase
	{
		public long ProfileId { get; set; }
		public long MarketItemId { get; set; }
	}
}
