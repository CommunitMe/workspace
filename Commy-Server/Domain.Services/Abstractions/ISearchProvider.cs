using Domain.Models.Enums;
using Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Abstractions
{
    public interface ISearchProvider
    {
        Task<SearchResult?> GetSearchResults(string searchTerm, long communityId, string? scKeys);
        EntityType GetSearchResultType();
    }
}
