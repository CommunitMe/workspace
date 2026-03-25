using Domain.Models;
using Domain.Models.Enums;
using Domain.Services.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<ISearchProvider> searchServices;
        public SearchService(IEnumerable<ISearchProvider> searchServices)
        {
            this.searchServices = searchServices;
        }

        public async Task<List<SearchResult>> GetSearchResults(string searchTerm, List<EntityType> entityTypes, long communityId, string? scKeys)
        {
            List<SearchResult> results = new List<SearchResult>();
            foreach(var entityType in entityTypes)
            {
                foreach (var service in searchServices)
                {
                    if (entityType == EntityType.AllCategories || service.GetSearchResultType() == entityType)
                    {
                        var serviceResults = await service.GetSearchResults(searchTerm, communityId, scKeys);
                        if (serviceResults != null)
                        {
                            results.Add(serviceResults);
                        }
                    }
                }
            }            

            return results;
        }
    }
}
