using ManifestImportExportAPI.Models;
using System;
using System.Collections.Generic;

namespace ManifestImportExportAPI.Repositories
{
    public interface IImportManifestRepository
    {
        RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(string Json, int depotnumber, bool scottishManifest);
    }
}