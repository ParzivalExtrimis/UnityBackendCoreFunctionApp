using Azure.Storage.Blobs;
using System;
using System.Collections.Generic;

namespace UnityBackendCoreFunctionApp.Models;

[Serializable]
public class CopierInput {
    public string container { get; set; }
    public ContentData data { get; set; }

    public CopierInput(string container, ContentData data) {
        this.container = container;
        this.data = data;
    }
}

