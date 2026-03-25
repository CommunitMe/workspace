using Domain.Models;
using Domain.Models.Abstractions.Transactions;
using Domain.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services
{
    public class LanguagesService : ILanguageService
    {
        private readonly ILogger<LanguagesService> logger;
        private readonly IDatabaseTransaction db;
        public LanguagesService(ILogger<LanguagesService> logger, IDatabaseTransaction db) 
        {
            this.logger = logger;
            this.db = db;
        }

        public async Task<IEnumerable<Language>> GetLanguages()
        {
            List<Language> LanguagesList = await db.Languages.GetAll();

            return LanguagesList.AsEnumerable();
        }
    }
}
