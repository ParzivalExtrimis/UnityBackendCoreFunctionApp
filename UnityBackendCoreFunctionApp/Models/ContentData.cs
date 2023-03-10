using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnityBackendCoreFunctionApp.Models {
    public class ContentData {
        public string id { get; set; }
        public string batch { get; set; }
        public string grade { get; set; }
        public string school { get; set; }
        public List<string> subjects { get; set; }
        public List<string> chapters { get; set; }
        public List<string> content { get; set; }
    }

}
