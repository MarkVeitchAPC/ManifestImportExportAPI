using ManifestImportExportAPI.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ManifestImportExportAPI.Repositories
{
    public interface IImportManifestRepository
    {
        //18-05-2016 RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(string Json, int depotnumber, bool scottishManifest);
        RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(JObject Json);
    }
}