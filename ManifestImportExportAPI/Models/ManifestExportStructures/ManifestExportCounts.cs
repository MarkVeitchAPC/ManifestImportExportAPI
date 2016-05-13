using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models
{
    public class ManifestExportCounts
    {
        public Int32 NoOfUnpricedCons { get; private set; }
        public Int32 NoOfUnmigratedJobs { get; private set; }

        public ManifestExportCounts(int noOfUnpricedCons, int noOfUnmigratedJobs)
        {
            NoOfUnpricedCons = noOfUnpricedCons;
            NoOfUnmigratedJobs = noOfUnmigratedJobs;
        }

        public ManifestExportCounts()
        {
        }
    }
}