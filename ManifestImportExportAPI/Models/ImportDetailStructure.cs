using System;

namespace ManifestImportExportAPI.Models
{
    public class ImportDetailStructure
    {
        public DateTime EnglishManifestLoadTimeStamp { get; private set; }
        public int RecordCount { get; private set; }
        public int EnglishCount { get; private set; }
        public int ScottishCount { get; private set; }
        public int ImportProgress { get; private set; }
        public int EnglishImportedCount { get; private set; }
        public int ScottishImportedCount { get; private set; }
        public int EnglishDuplicateCount { get; private set; }
        public int ScottishDuplicateCount { get; private set; }
        public DateTime ScottishManifestLoadTimeStamp { get; private set; }
        public DateTime LastExportedTimestamp { get; private set; }

        public ImportDetailStructure(DateTime englishManifestLoadTimeStamp,
                                     int recordCount,
                                     int englishCount,
                                     int scottishCount,
                                     int importProgress,
                                     int englishImportedCount,
                                     int scottishImportedCount,
                                     int englishDuplicateCount,
                                     int scottishDuplicateCount,
                                     DateTime scottishManifestLoadTimeStamp,
                                     DateTime lastExportedTimestamp
                                     )
        {
            EnglishManifestLoadTimeStamp = englishManifestLoadTimeStamp;
            RecordCount = recordCount;
            EnglishCount = englishCount;
            ScottishCount = scottishCount;
            ImportProgress = importProgress;
            EnglishImportedCount = englishImportedCount;
            ScottishImportedCount = scottishImportedCount;
            EnglishDuplicateCount = englishDuplicateCount;
            ScottishDuplicateCount = scottishDuplicateCount;
            ScottishManifestLoadTimeStamp = scottishManifestLoadTimeStamp;
            LastExportedTimestamp = lastExportedTimestamp;
    }

        public ImportDetailStructure()
        { }
    }
}