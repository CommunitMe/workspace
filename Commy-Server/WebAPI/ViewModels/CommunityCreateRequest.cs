using Mapster;

namespace WebAPI.ViewModels
{
	[AdaptTo(typeof(Domain.Models.Community))]
	public class CommunityCreateRequest
	{
		public string Name { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
	}
}
