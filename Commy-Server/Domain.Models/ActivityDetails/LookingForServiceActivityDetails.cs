using Domain.Models.Enums;

namespace Domain.Models.ActivityDetails
{
	public class LookingForServiceActivityDetails : ActivityDetailsBase
	{
		public long ProfileId { get; set; }
		public ServiceDuration ServiceDuration { get; set; } = null!;
		public ServiceType ServiceType { get; set; } = null!;
	}
}
