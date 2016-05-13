using System;
using System.Collections.Immutable;
using System.Linq;

namespace ManifestImportExportAPI.Models
{
    public class ManifestExport
    {
        private ManifestExport manifestExport;

        public string DocNum { get; private set; }
        public string Date { get; private set; }
        public string AccountNo { get; private set; }
        public string Contact { get; private set; }
        public string Requestdep { get; private set; }
        public string Senddep { get; private set; }
        public string Deliverdep { get; private set; }
        public string Consignor { get; private set; }
        public string Conortel { get; private set; }
        public string Conoraddr1 { get; private set; }
        public string Conoraddr2 { get; private set; }
        public string Conoraddr3 { get; private set; }
        public string Conoraddr4 { get; private set; }
        public string Conorpcode { get; private set; }
        public string Conorref { get; private set; }
        public string Consignee { get; private set; }
        public string Coneetel { get; private set; }
        public string Coneeaddr1 { get; private set; }
        public string Coneeaddr2 { get; private set; }
        public string Coneeaddr3 { get; private set; }
        public string Coneeaddr4 { get; private set; }
        public string Coneepcode { get; private set; }
        public string Coneefao { get; private set; }
        public string Service { get; private set; }
        public string Cartons { get; private set; }
        public string Weight { get; private set; }
        public string Volume { get; private set; }
        public string Security { get; private set; }
        public string Insurvalue { get; private set; }
        public string Specialins { get; private set; }
        public string Delivprice { get; private set; }
        public string Route { get; private set; }
        public string Identifier { get; private set; }
        public string Mintime { get; private set; }
        public string Maxtime { get; private set; }

        public ManifestExport(string docnum,
                              string date,
                              string accountno,
                              string contact,
                              string requestdep,
                              string senddep,
                              string deliverdep,
                              string consignor,
                              string conortel,
                              string conoraddr1,
                              string conoraddr2,
                              string conoraddr3,
                              string conoraddr4,
                              string conorpcode,
                              string conorref,
                              string consignee,
                              string coneetel,
                              string coneeaddr1,
                              string coneeaddr2,
                              string coneeaddr3,
                              string coneeaddr4,
                              string coneepcode,
                              string coneefao,
                              string service,
                              string cartons,
                              string weight,
                              string volume,
                              string security,
                              string insurvalue,
                              string specialins,
                              string delivprice,
                              string route,
                              string identifier,
                              string mintime,
                              string maxtime)
        {
            DocNum = docnum;
            Date = date;
            AccountNo = accountno;
            Contact = contact;
            Requestdep = requestdep;
            Senddep = senddep;
            Deliverdep = deliverdep;
            Consignor = consignor;
            Conortel = conortel;
            Conoraddr1 = conoraddr1;
            Conoraddr2 = conoraddr2;
            Conoraddr3 = conoraddr3;
            Conoraddr4 = conoraddr4;
            Conorpcode = conorpcode;
            Conorref = conorref;
            Consignee = consignee;
            Coneetel = coneetel;
            Coneeaddr1 = coneeaddr1;
            Coneeaddr2 = coneeaddr2;
            Coneeaddr3 = coneeaddr3;
            Coneeaddr4 = coneeaddr4;
            Coneepcode = coneepcode;
            Coneefao = coneefao;
            Service = service;
            Cartons = cartons;
            Weight = weight;
            Volume = volume;
            Security = security;
            Insurvalue = insurvalue;
            Specialins = specialins;
            Delivprice = delivprice;
            Route = route;
            Identifier = identifier;
            Mintime = mintime;
            Maxtime = maxtime;
        }

        public ManifestExport()
        { }

        public ManifestExport(ManifestExport manifestExport)
        {
            this.manifestExport = manifestExport;
        }

        public static ManifestExport operator +(ManifestExport x, ManifestExport y)
        {
            return new ManifestExport(x + y);
        }

        //public static ImmutableList<ManifestExport> Combine(ImmutableList<ManifestExport> listA, ImmutableList<ManifestExport> listB)
        //{
        //    var combined = listA.AddRange(listB);
        //    return Reduce(combined);
        //}

        //public static ImmutableList<ManifestExport> Reduce(IImmutableList<ManifestExport> cons)
        //{
        //    var reduce = cons.GroupBy(x => x.DepotNumber).
        //       Select(x => x.ToImmutableList()).
        //       Select(x => x.Aggregate((y, z) => y + z)).ToImmutableList();
        //    return reduce;
        //}
    }
}  