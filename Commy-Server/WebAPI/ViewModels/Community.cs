using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.Community))]
    public class Community
    {
        [AdaptMember(nameof(Domain.Models.Community.Id))]
        public string Cid { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageURI { get; set; } = string.Empty;
    }
}
