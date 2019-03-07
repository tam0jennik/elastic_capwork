using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace website.DataLayer.Models
{
    public class PlayModel : IEntity
    {
        public string LineId { get; set; }
        public string LineNumber { get; set; }
        public string PlayName { get; set; }
        public string Speaker { get; set; }
        public string SpeechNumber { get; set; }
        public string TextEntry { get; set; }
        public string Id { get; set; }
    }
}
