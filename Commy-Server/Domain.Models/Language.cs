using Domain.Abstractions.Common;
using Domain.Models.Criteria;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Language : IPersistantModel<Language>
    {
        public short Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public ICriterion<Language> GetCriterion()
        {
            return new LanguageCriterion { Id = Id };
        }
    }
}
