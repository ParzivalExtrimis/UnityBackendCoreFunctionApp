using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityBackendCoreFunctionApp.Models;
[Serializable]
public class ContentData {
    public string id { get; set; }
    public string batch { get; set; }
    public string grade { get; set; }
    public string school { get; set; }
    public List<string> subjects { get; set; }
    public List<string> chapters { get; set; }
    public List<string> content { get; set; }

}


