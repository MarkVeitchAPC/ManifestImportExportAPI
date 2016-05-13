using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestImportExportAPI.Models.ManifestImportStructures
{
    public class ManifestImportContact
    {
        public String ContactName { get; private set; }
        public String ContactTelephone { get; private set; }
        public String ContactMobilePhone { get; private set; }
        public String ContactFaxNumber { get; private set; }
        public String ContactEMailAddress { get; private set; }
        public Boolean ContactEmailAlert { get; private set; }
        public Boolean Active { get; private set; }

        public ManifestImportContact(string mIContactName, 
                                     string mIContactTelephone,
                                     string mIContactMobilePhone,
                                     string mIContactFaxNumber,
                                     string mIContactEMailAddress,
                                     Boolean mIContactEmailAlert,
                                     Boolean mIActive)
        {
            ContactName = mIContactName;
            ContactTelephone = mIContactTelephone;
            ContactMobilePhone = mIContactMobilePhone;
            ContactFaxNumber = mIContactFaxNumber;
            ContactEMailAddress = mIContactEMailAddress;
            ContactEmailAlert = mIContactEmailAlert;
            Active = mIActive;
        }

        public ManifestImportContact()
        { }
    }
}