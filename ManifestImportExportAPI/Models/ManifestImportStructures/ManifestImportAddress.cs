using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models.ManifestImportStructures
{
    public class ManifestImportAddress
    {
        public string CompanyName { get; private set; }
        public string BuildingName { get; private set; }
        public string BuildingNumber { get; private set; }
        public string SubBuildingName { get; private set; }
        public string SubBuildingNumber { get; private set; }
        public string AddressLine1 { get; private set; }
        public string AddressLine2 { get; private set; }
        public string Town { get; private set; }
        public string County { get; private set; }
        public string PostCode { get; private set; }
        public short CountryID { get; private set; }
        public bool Active { get; private set; }
        public bool ConeeQas { get; private set; }

        public ManifestImportAddress(string mIcompanyName, string mIbuildingName, string mIbuildingNumber, string mIsubBuildingName,
                                     string mIsubBuildingNumber, string mIaddressLine1, string mIaddressLine2, string mItown,
                                     string mIcounty, string mIpostCode, short mICountryID, bool mIActive, bool mIConeeQas)
        {
            CompanyName = mIcompanyName;
            BuildingName = mIbuildingName;
            BuildingNumber = mIbuildingNumber;
            SubBuildingName = mIsubBuildingName;
            SubBuildingNumber = mIsubBuildingNumber;
            AddressLine1 = mIaddressLine1;
            AddressLine2 = mIaddressLine2;
            Town = mItown;
            County = mIcounty;
            PostCode = mIpostCode;
            CountryID = mICountryID;
            Active = mIActive;
            ConeeQas = mIConeeQas;
        }
    }
}