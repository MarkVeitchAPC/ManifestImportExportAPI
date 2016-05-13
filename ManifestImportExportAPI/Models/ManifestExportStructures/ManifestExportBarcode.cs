using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models
{
    public class ManifestExportBarcode
    {
        public string Identifier { get; private set; }
        public string Barcode { get; private set; }

        public ManifestExportBarcode(string identifier, string barcode)
        {
            Identifier = identifier;
            Barcode = barcode;
        }
    }
}