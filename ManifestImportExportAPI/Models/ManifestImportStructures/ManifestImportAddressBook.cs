using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models.ManifestImportStructures
{
    public class ManifestImportAddressBook
    {
        public string MIDeliveryRef { get; private set; }
        public string MISpecialInstructions { get; private set; }

        public ManifestImportAddressBook(string mIDeliveryRef, string mISpecialInstructions)
        {
            MIDeliveryRef = mIDeliveryRef;
            MISpecialInstructions = mISpecialInstructions;
        }

        public ManifestImportAddressBook()
        {

        }
    }
}