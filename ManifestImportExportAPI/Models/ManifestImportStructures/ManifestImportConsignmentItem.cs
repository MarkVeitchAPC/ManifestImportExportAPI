using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models.ManifestImportStructures
{
    public class ManifestImportConsignmentItem
    {
        public int ItemStatus { get; private set; }
        public int ItemNumber { get; private set; }
        public Boolean IsVolumetric { get; private set; }
        public string ShortReference { get; private set; }
        public string CustomerReference1 { get; private set; }
        public string CustomerReference2 { get; private set; }
        public Boolean AutoPrintLabel { get; private set; }
        public Boolean LabelPrinted { get; private set; }
        public Decimal Weight { get; private set; }
        public Decimal Lsize { get; private set; }
        public Decimal Wsize { get; private set; }
        public Decimal Hsize { get; private set; }
        public Decimal TrueWeight { get; private set; }

        public ManifestImportConsignmentItem(int mIItemStatus, 
                                             int mIItemNumber,
                                             Boolean mIIsVolumetric,
                                             string mIShortReference, 
                                             string mICustomerReference1,
                                             string mICustomerReference2, 
                                             Boolean mIAutoPrintLabel, 
                                             Boolean mILabelPrinted, 
                                             Decimal mIWeight,
                                             Decimal mILsize, 
                                             Decimal mIWsize, 
                                             Decimal mIHsize, 
                                             Decimal mITrueWeight )
        {
            ItemStatus = mIItemStatus;
            ItemNumber = mIItemNumber;
            IsVolumetric = mIIsVolumetric;
            ShortReference = mIShortReference;
            CustomerReference1 = mICustomerReference1;
            CustomerReference2 = mICustomerReference2;
            AutoPrintLabel = mIAutoPrintLabel;
            LabelPrinted = mILabelPrinted;
            Weight = mIWeight;
            Lsize = mILsize;
            Wsize = mIWsize;
            Hsize = mIHsize;
            TrueWeight = mITrueWeight;
        }

        public ManifestImportConsignmentItem()
        { }
    }
}