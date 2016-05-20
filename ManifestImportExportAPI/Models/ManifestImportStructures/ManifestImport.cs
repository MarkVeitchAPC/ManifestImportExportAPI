using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ManifestImportExportAPI.Models.ManifestImportStructures;
using Newtonsoft.Json.Linq;

namespace ManifestImportExportAPI.Models
{
    public class ManifestImport
    {
        //public Boolean QuarantineConsignment { get; private set; }
        public int manifestImportDepotNumber { get; private set; }
        public Boolean manifestScottish { get; private set; }
        public ManifestImportConsignmentHeader conHeader { get; private set; }
        public ManifestImportAddress conCollAddress { get; private set; }
        public ManifestImportAddress conDelAddress { get; private set; }
        public ManifestImportContact conCollContact { get; private set; }
        public ManifestImportContact conDelContact { get; private set; }
        public ManifestImportConsignmentItem conItem { get; private set; }
        public ManifestImportComment conComment { get; private set; }
        public ManifestImportConsignorPricing consPricing { get; private set; }

        public ManifestImport(
                              int mIDepotNumber, 
                              Boolean miScottish, 
                              //Boolean miQuarantineConsignment,    
                              ManifestImportConsignmentHeader mIConsignmentHeader,
                              ManifestImportAddress mIConsignmentCollAddress,
                              ManifestImportAddress mIConsignmentDelAddress,
                              ManifestImportContact mIConsignmentCollContact,
                              ManifestImportContact mIConsignmentDelContact,
                              ManifestImportConsignmentItem mIConsignmentItem,
                              ManifestImportComment mIComment, 
                              ManifestImportConsignorPricing mIConsignorPricing)
        {
            manifestImportDepotNumber = mIDepotNumber;
            manifestScottish = miScottish;
            //QuarantineConsignment = miQuarantineConsignment;
            conHeader = mIConsignmentHeader;
            conCollAddress = mIConsignmentCollAddress;
            conDelAddress = mIConsignmentDelAddress;
            conCollContact = mIConsignmentCollContact;
            conDelContact = mIConsignmentDelContact;
            conItem = mIConsignmentItem;
            conComment = mIComment;
            consPricing = mIConsignorPricing; 
        }

        public ManifestImport(JToken json)
        {
            manifestImportDepotNumber = (int)json["depotNumber"];
            manifestScottish = (bool)json["scottishManifest"];
            //QuarantineConsignment = (bool)json["QuarantinedConsignment"];
            conHeader = ParseManifestImportConsignmentHeader(json);
            conCollAddress = ParseManifestImportAddress(json);
            conDelAddress = ParseManifestImportAddress(json);
            conCollContact = ParseManifestImportContact(json);
            conDelContact = ParseManifestImportContact(json);
            conItem = ParseManifestImportConsignmentItem(json);
            conComment = ParseManifestImportComment(json);
            consPricing = ParseManifestImportConsignorPricing(json);
        }

        public ManifestImport()
        { }

        private ManifestImportConsignorPricing ParseManifestImportConsignorPricing(JToken p)
        {
            throw new NotImplementedException();
        }

        private ManifestImportComment ParseManifestImportComment(JToken p)
        {

            throw new NotImplementedException();
        }

        private ManifestImportConsignmentItem ParseManifestImportConsignmentItem(JToken p)
        {
            throw new NotImplementedException();
        }

        private ManifestImportContact ParseManifestImportContact(JToken p)
        {
            throw new NotImplementedException();
        }

        private ManifestImportAddress ParseManifestImportAddress(JToken p)
        {
            throw new NotImplementedException();
        }

        private ManifestImportConsignmentHeader ParseManifestImportConsignmentHeader(JToken p)
        {
            throw new NotImplementedException();
        }
    }
}