using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models.ManifestImportStructures
{
    public class ManifestImportConsignorPricing
    {
        public Decimal Price { get; private set;  }
        public int InvoiceNumber { get; private set; }
        public Boolean Verified { get; private set; }
        public Boolean ManuallyPriced { get; private set; }

        public ManifestImportConsignorPricing(Decimal mIPrice,
                                              int mIInvoiceNumber,
                                              Boolean mIVerified,
                                              Boolean mIManuallyPriced)
        {
            Price = mIPrice;
            InvoiceNumber = mIInvoiceNumber;
            Verified = mIVerified;
            ManuallyPriced = mIManuallyPriced;
        }

        public ManifestImportConsignorPricing()
        { }
    }
}