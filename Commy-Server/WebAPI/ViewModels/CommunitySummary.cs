using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.ViewModels
{
    public class CommunitySummary
    {
        public string Id { get; set; } = string.Empty;
        public int ProfilesCount { get; set; }
        public List<string> ProfilesIds { get; set; } = new List<string>();
    }
}
