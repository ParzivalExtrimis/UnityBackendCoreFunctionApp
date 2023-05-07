using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityBackendCoreFunctionApp.Models;
[Serializable]
public class ContentData {
    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; }

    [JsonProperty(PropertyName = "department")]
    public string Department { get; set; }

    [JsonProperty(PropertyName = "school")]
    public string School { get; set; }

    [JsonProperty(PropertyName = "subjects")]
    public List<string> Subjects { get; set; }

    [JsonProperty(PropertyName = "chapters")]
    public List<string> Chapters { get; set; }

    [JsonProperty(PropertyName = "content")]
    public List<string> Content { get; set; }
}


