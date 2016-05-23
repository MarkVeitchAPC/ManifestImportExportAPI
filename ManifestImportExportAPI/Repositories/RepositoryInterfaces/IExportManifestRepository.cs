using ManifestImportExportAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Repositories.RepositoryInterfaces
{
    public interface IExportManifestRepository
    {
        //goto immutablelist version  
        //RetrieveResults<List<ManifestExport>> ExportManifest(Int32 depotNumber, DateTime manifestDate);

        RetrieveResults<ManifestExport> ExportManifest(Int32 depotNumber, DateTime manifestDate);
        RetrieveResults<ManifestExportBarcode> ExportManifestBarcode(Int32 depotNumber, DateTime manifestDate);
        RetrieveResults<ManifestImportDetailUpdateFailed> ManifestImportDetailUpdate(string json);
        RetrieveResults<ManifestExportCounts> ManifestExportCounts();
    }
}