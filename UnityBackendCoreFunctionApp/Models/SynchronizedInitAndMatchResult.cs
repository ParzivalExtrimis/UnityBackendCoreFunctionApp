using Azure.Storage.Blobs;
using System.Collections.Generic;

namespace UnityBackendCoreFunctionApp.Models;
public class SynchronizedInitAndMatchResult {
     
    public bool Result { set; get; }
    public List<string> Data { get; set; }
    public string UserContainerclient { get; set; }
}
