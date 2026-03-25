using Domain.Models;

namespace Domain.Services.Abstractions
{
    public interface ILanguageService
    {
        Task<IEnumerable<Language>> GetLanguages();
    }
}
