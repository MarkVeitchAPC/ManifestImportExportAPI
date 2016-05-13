using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Enums
{
    public enum ManifestImportProgress
    {
        Unknown = 0,
        ReadyToImport,
        Importing,
        Imported,
        Finished,
        OlderFile,
        NewFile,
        Failed,
        FileNotFound,
        Errored,
        Duplicate
    }

    public enum InsertOrUpdate
    {
        insert = 0,
        update
    }

    public enum ErrorCodes
    {
        //Generic No Errors
        None,
        ImportFailed
    }
}