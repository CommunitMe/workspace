namespace Domain.Models.ActivityDetails
{
	public class NewServiceProviderActivityDetails : ActivityDetailsBase
	{

		public long ProfileId { get; set; }
		public long CommunityId { get; set; }
		public ServiceType ServiceType { get; set; }
	}
}
