using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models.ManifestImportStructures
{
    public class ManifestImportComment
    {
        public int ConnectVisibilityID { get; private set; }
        public string Comment { get; private set; }

        public ManifestImportComment(int mIConnectVisibilityID, string mIComment)
        {
            ConnectVisibilityID = mIConnectVisibilityID;
            Comment = mIComment;
        }

        public ManifestImportComment()
        { }
    }
}