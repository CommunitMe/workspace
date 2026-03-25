using Mapster;

namespace WebAPI.ViewModels
{
    [AdaptTwoWays(typeof(Domain.Models.Currency))]
    public class Currency
    {
        public short Id { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
