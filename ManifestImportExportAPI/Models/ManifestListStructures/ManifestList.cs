using ManifestImportExportAPI.Enums;
using System;
using System.Collections.Immutable;
using System.Linq;

//*********************************************************************************
//**
//** IT IS LIKELY THAT THIS PART OF THE WEBSERVICE WILL NOT BE PUT INTO ACTION 
//** THE CURRENT THINKING IS THAT THE RETREIVE LATEST MANIFEST LIST WILL BE
//** DONE ON THE CLIENT SIDE ON WWW2.  THIS REMAINS HERE FOR POSSIBLE FUTURE USE 
//**
//********************************************************************************* 

namespace ManifestImportExportAPI.Models
{
    public class ManifestList
    {
        private ManifestList manifestList;

        public string DepotNumber { get; set; }
        public DateTime EnglishManifestLoadTimeStamp { get; set; }
        public DateTime ScottishManifestLoadTimeStamp { get; set; }
        public int EnglishCount { get; set; }
        public int ScottishCount { get; set; }
        public ManifestImportProgress ImportProgress { get; set; }
        public string ExtraInformation { get; set; }
        public int EnglishImportedCount { get; set; }
        public int ScottishImportedCount { get; set; }
        public int EnglishDuplicateCount { get; set; }
        public int ScottishDuplicateCount { get; set; }
        public DateTime? EnglishFileTimestamp { get; set; }
        public DateTime? ScottishFileTimestamp { get; set; }

        public ManifestList(string depotNumber,
                            DateTime englishManifestLoadTimeStamp,
                            DateTime scottishManifestLoadTimeStamp, 
                            int englishCount,
                            int scottishCount,
                            ManifestImportProgress importProgress,
                            string extraInformation,
                            int englishImportedCount,
                            int scottishImportedCount,
                            int englishDuplicateCount,
                            int scottishDuplicateCount,
                            DateTime? englishFileTimestamp, 
                            DateTime? scottishFileTimestamp)
        {
            DepotNumber = depotNumber;
            EnglishManifestLoadTimeStamp = englishManifestLoadTimeStamp;
            ScottishManifestLoadTimeStamp = scottishManifestLoadTimeStamp;
            EnglishCount = englishCount;
            ScottishCount = scottishCount;
            ImportProgress = importProgress;
            ExtraInformation = extraInformation;
            EnglishImportedCount = englishImportedCount;
            ScottishImportedCount = scottishImportedCount;
            EnglishDuplicateCount = englishDuplicateCount;
            ScottishDuplicateCount = scottishDuplicateCount;
            EnglishFileTimestamp = englishFileTimestamp;
            ScottishFileTimestamp = scottishFileTimestamp;
        }

        public ManifestList(ManifestList manifestList)
        {
            this.manifestList = manifestList;
        }

        public ManifestList()
        {
        }

        public static ManifestList operator +(ManifestList x, ManifestList y)
        {
            return new ManifestList(x + y);
        }

        public static ImmutableList<ManifestList> Combine(ImmutableList<ManifestList> listA, ImmutableList<ManifestList> listB)
        {
            var combined = listA.AddRange(listB);
            return Reduce(combined);
        }

        public static ImmutableList<ManifestList> Reduce(IImmutableList<ManifestList> cons)
        {
            var reduce = cons.GroupBy(x => x.DepotNumber).
               Select(x => x.ToImmutableList()).
               Select(x => x.Aggregate((y, z) => y + z)).ToImmutableList();
            return reduce;
        }
    }


}