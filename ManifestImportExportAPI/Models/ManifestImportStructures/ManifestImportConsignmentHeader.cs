using System;

namespace ManifestImportExportAPI.Models
{
    public class ManifestImportConsignmentHeader
    {
        public string ConNumber { get; private set; }
        public string AccountNo { get; private set;  }
        public int ReqDepot { get; private set; }
        public int SendDepot { get; private set; }
        public int DelivDepot { get; private set; }
        //Consignor { get; private set; }
        public string Service { get; private set; }
        public DateTime CollDate { get; private set; }
        public DateTime SendDate { get; private set; }
        public string ConorRef { get; private set; }
        public string Identifier { get; private set; }
        public int RouteId { get; private set; }
        public string ClaimRef { get; private set; }
        public Boolean Security { get; private set; }
        public int GoodsTypeId { get; private set; }
        public string SpecialInstructions { get; private set; }
        public DateTime GoodsReadyFromTime { get; private set; }
        public DateTime GoodsReadyToTime { get; private set; }
        public decimal LiabilityValue { get; private set; }
        public Boolean Fragile { get; private set; }
        public Boolean NonConveyorable { get; private set; }
        public Boolean IsVolumetric { get; private set; }
        public decimal Weight { get; private set; }
        public decimal TrueWeight { get; private set; }
        public string DiscrepancyCode { get; private set; }
        public string DeliveryRef { get; private set; }
        public string EntryType { get; private set; }
        public string ConeeFao { get; private set; } 
        public int Cartons { get; private set; }

        public ManifestImportConsignmentHeader(string mIConsignmentNumber,
                                               string miAccountNo,
                                               int mIRequestDepot,
                                               int mISendingDepot,
                                               int mIDeliveryDepot,
                                               //Consignor { get; private set; }
                                               string mIService,
                                               DateTime mICollectionDate,
                                               DateTime mISendDate,
                                               string mIConorRef,
                                               string mIIdentifier,
                                               int mIRouteId,
                                               string mIClaimRef,
                                               Boolean mISecurity,
                                               int mIGoodsTypeId,
                                               string mISpecialInstructions,
                                               DateTime mIGoodsReadyFromTime,
                                               DateTime mIGoodsReadyToTime,
                                               decimal mILiabilityValue,
                                               Boolean mIFragile,
                                               Boolean mINonConveyorable,
                                               Boolean mIIsVolumetric,
                                               decimal mIWeight,
                                               decimal mITrueWeight,
                                               string mIDiscrepancyCode,
                                               string mIDeliveryRef,
                                               string mIEntryType,
                                               string mIConeeFao,
                                               int mICartons)
        {
            ConNumber = mIConsignmentNumber;
            AccountNo = miAccountNo;
            ReqDepot = mIRequestDepot;
            SendDepot = mISendingDepot;
            DelivDepot = mIDeliveryDepot;
            //Consignor { get; private set; }
            Service = mIService;
            CollDate = mICollectionDate;
            SendDate = mISendDate;
            ConorRef = mIConorRef;
            Identifier = mIIdentifier;
            RouteId = mIRouteId;
            ClaimRef = mIClaimRef;
            Security = mISecurity;
            GoodsTypeId = mIGoodsTypeId;
            SpecialInstructions = mISpecialInstructions;
            GoodsReadyFromTime = mIGoodsReadyFromTime;
            GoodsReadyToTime = mIGoodsReadyToTime;
            LiabilityValue = mILiabilityValue;
            Fragile = mIFragile;
            NonConveyorable = mINonConveyorable;
            IsVolumetric = mIIsVolumetric;
            Weight = mIWeight;
            TrueWeight = mITrueWeight;
            DiscrepancyCode = mIDiscrepancyCode;
            DeliveryRef = mIDeliveryRef;
            EntryType = mIEntryType;
            ConeeFao = mIConeeFao;
            Cartons = mICartons;
        }

        public ManifestImportConsignmentHeader()
        { }
    }
}