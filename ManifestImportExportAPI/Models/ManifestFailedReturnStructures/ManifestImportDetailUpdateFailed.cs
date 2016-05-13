using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models
{
    // Model required to allow updates of database called [Continuus].[dbo].[AutomatedManifestImportDetail] 
    // which holds import and export summaries.  This is used primarily for updating the LastExportDateTime
    //
    public class ManifestImportDetailUpdateFailed
    {
        public string DepotNo { get; private set; }
        public string ExportDateTime { get; private set; }

        public ManifestImportDetailUpdateFailed(string depotNo, string exportDateTime)
        {
            DepotNo = depotNo;
            ExportDateTime = exportDateTime;
        }
    }
}