using Domain.Models;
using Domain.Models.Enums;

namespace Domain.Services.Abstractions
{
    public interface ISearchService
    {
        Task<List<SearchResult>> GetSearchResults(string searchTerm, List<EntityType> entityTypes, long communityId, string? scKeys);
    }
}
