using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Services.Models
{
    public class ProfileCircleProperties
    {
        public int width { get; set; } = 40;
        public int height { get; set; } = 40;
        public string? bgColor { get; set; }
        public string fontFamily { get; set; } = "Poppins";
        public float? textSize { get; set;}
        public string textColor { get; set; } = "000000";
    }
}
