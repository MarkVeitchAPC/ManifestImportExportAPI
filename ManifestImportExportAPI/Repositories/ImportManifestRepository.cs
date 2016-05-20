using System;
using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Models.ManifestImportStructures;
using System.Collections.Immutable;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using ManifestImportExportAPI.Enums;
using System.Data.SqlClient;
using System.Data;
using ManifestImportExportAPI.Domain;
using NLog;
using System.Globalization;

namespace ManifestImportExportAPI.Repositories
{
    public class ImportManifestRepository : IImportManifestRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _connectionString_HubData_MySQL;
        private string _connectionString;
        private Guid _depotKey;
        private int _depotNumber;
        private bool _scottishManifest;
        private string _loadedBy;
        private int _scottishCount = 0;
        private int _englishCount = 0;
        private int _scottishImported = 0;
        private int _englishImported = 0;
        private int _scottishDuplicate = 0;
        private int _englishDuplicate = 0;
        private bool _scottishImportFailed = false;
        private bool _englishImportFailed = false;
        private int _importProgress = 0;


        public ImportManifestRepository()
        {
            //TODO set up both of these connection strings in the config
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["Main.ConnectionString"];
            _connectionString_HubData_MySQL = System.Configuration.ConfigurationManager.AppSettings["connectionString_HubData_MySQL"];
        }

        //18-05-2016 public RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(string Json, int depotnumber, bool scottishManifest)
        public RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(JObject Json)
        {
            //_depotNumber = depotNumber; 
            //_scottishManifest = scottishManifest;
            //_depotKey = GetImportManifestDepotKey(); 

            //var builderOut = ImmutableList.CreateBuilder<List<ManifestImportFailed>>();
            var builderOut = ImmutableList.CreateBuilder<ManifestImportDetailUpdateFailed>();
            var builderIn = ImmutableList.CreateBuilder<ManifestImport>();

            var queryStatus = QueryStatus.OK;
            var lastHeaderKey = string.Empty;

            ManifestImportFailed manifest = new ManifestImportFailed();

            foreach (var item in Json)
            {
                if (item.Key == "parameters")
                {
                    var param = new JArray(item.Value);
                    param = JArray.Parse(item.Value.ToString());
                    foreach (var field in param.Children())
                    {
                        var itemProperties = field.Children<JProperty>();
                        _depotNumber = (int)itemProperties.FirstOrDefault(x => x.Name == "depotNumber");
                        _scottishManifest = (bool)itemProperties.FirstOrDefault(x => x.Name == "isScottish");
                        _loadedBy = (string)itemProperties.FirstOrDefault(x => x.Name == "loadedBy");
                    }
                }
            }

            foreach (var item in Json)
            { 
                if (item.Key == "data")
                {
                    var data = new JArray(item.Value);
                    data = JArray.Parse(item.Value.ToString());
                    foreach (var field in data.Children())
                    {
                        var itemProperties = field.Children<JProperty>();

                        var dt = GetSendDate(itemProperties.FirstOrDefault(x => x.Name == "DATE").Value.ToString());

                        var mi = new ManifestImport(_depotNumber, _scottishManifest,
                                                    new ManifestImportConsignmentHeader((string)itemProperties.FirstOrDefault(x => x.Name == "DOCNUM"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "OLDACCNO"),
                                                                                        (int)itemProperties.FirstOrDefault(x => x.Name == "REQUESTDEP"),
                                                                                        (int)itemProperties.FirstOrDefault(x => x.Name == "SENDDEP"),
                                                                                        (int)itemProperties.FirstOrDefault(x => x.Name == "DELIVERDEP"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "SERVICE"),
                                                                                        //(DateTime)itemProperties.FirstOrDefault(x => x.Name == "COLLDATE"),
                                                                                        DateTime.MinValue, // CollDate
                                                                                        GetSendDate(itemProperties.FirstOrDefault(x => x.Name == "DATE").Value.ToString()),
                                                                                        //DateTime.ParseExact((itemProperties.FirstOrDefault(x => x.Name == "DATE").ToString()), "MM/dd/yyyy hh:mm:ss ttt", CultureInfo.InvariantCulture), 
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORREF"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "IDENTIFIER"),
                                                                                        GetRouteID(itemProperties.FirstOrDefault(x => x.Name == "ROUTE").Value.ToString()), 
                                                                                        null, // ClaimsRef
                                                                                              //(bool)itemProperties.FirstOrDefault(x => x.Name == "SECURITY"),
                                                                                        (itemProperties.FirstOrDefault(x => x.Name == "SECURITY")).ToString() == "1" ? true : false,

                                                                                        0, // GoodsTypeId
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "SPECIALINS"),
                                                                                        DateTime.MinValue, // GoodsReadyTimeFrom
                                                                                        DateTime.MinValue, // GoodsReadyTimeTo
                                                                                        (Decimal)itemProperties.FirstOrDefault(x => x.Name == "INSURVALUE"),
                                                                                        //(bool)itemProperties.FirstOrDefault(x => x.Name == "FRAGILE"),
                                                                                        (itemProperties.FirstOrDefault(x => x.Name == "FRAGILE")).ToString() == "1" ? true : false,
                                                                                        false, // NonConveyorable
                                                                                               //(bool)itemProperties.FirstOrDefault(x => x.Name == "VOLUME"),
                                                                                        (itemProperties.FirstOrDefault(x => x.Name == "VOLUME")).ToString() == "1" ? true : false,
                                                                                        (Decimal)itemProperties.FirstOrDefault(x => x.Name == "WEIGHT"),
                                                                                        (Decimal)itemProperties.FirstOrDefault(x => x.Name == "TRUEWEIGHT"),
                                                                                        string.Empty, // DiscrepCode
                                                                                        null, // ConneeRef
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "ENTRYTYPE"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEEFAO"),
                                                                                        (int)itemProperties.FirstOrDefault(x => x.Name == "CARTONS")),

                                                    new ManifestImportAddress((string)itemProperties.FirstOrDefault(x => x.Name == "CONSIGNOR"),
                                                                                        null, null, null, null,
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORADDR1"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORADDR2"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORADDR3"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORADDR4"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORPCODE"),
                                                                                        0, false,
                                                                                        //(Boolean)itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")),
                                                                                        (itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")).ToString() == "1" ? true : false),

                                                    new ManifestImportAddress((string)itemProperties.FirstOrDefault(x => x.Name == "CONSIGNOR"),
                                                                                        null, null, null, null,
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR1"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR2"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR3"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR4"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEEPCODE"),
                                                                                        0, false,
                                                                                        //(Boolean)itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")),
                                                                                        (itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")).ToString() == "1" ? true : false),

                                                    new ManifestImportContact((string)itemProperties.FirstOrDefault(x => x.Name == "CONTACT"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONORTEL"),
                                                                                        (string)string.Empty,  // Mobile Number
                                                                                        (string)string.Empty,  // Fax Number 
                                                                                        (string)string.Empty,  // Email Address
                                                                                        false,                 // Email alert
                                                                                        false),                // Active 

                                                    new ManifestImportContact((string)itemProperties.FirstOrDefault(x => x.Name == "CONSIGNEE"),
                                                                                        (string)itemProperties.FirstOrDefault(x => x.Name == "CONEETEL"),
                                                                                        (string)string.Empty,  // Mobile Number
                                                                                        (string)string.Empty,  // Fax Number 
                                                                                        (string)string.Empty,  // Email Address
                                                                                        false,                 // Email alert
                                                                                        false),                // Active 

                                                    new ManifestImportConsignmentItem(0,                    // ItemStatus 
                                                                                      0,                    // Item Number
                                                                                      //(bool)itemProperties.FirstOrDefault(x => x.Name == "VOLUME"),
                                                                                      (itemProperties.FirstOrDefault(x => x.Name == "VOLUME")).ToString() == "1" ? true : false,
                                                                                      (string)string.Empty, // ShortRef
                                                                                      (string)string.Empty, // CustomerRef1
                                                                                      (string)string.Empty, // CustomerRef2 
                                                                                      false,                // AutuPrintedLabel
                                                                                      false,                // LabelPrinted
                                                                                      (Decimal)itemProperties.FirstOrDefault(x => x.Name == "WEIGHT"),
                                                                                      (Decimal)itemProperties.FirstOrDefault(x => x.Name == "LSIZE"),
                                                                                      (Decimal)itemProperties.FirstOrDefault(x => x.Name == "WSIZE"),
                                                                                      (Decimal)itemProperties.FirstOrDefault(x => x.Name == "HSIZE"),
                                                                                      (Decimal)itemProperties.FirstOrDefault(x => x.Name == "TRUEWEIGHT")),
                                                    new ManifestImportComment(0,                            // CommentVisibityId
                                                                             (string)string.Empty),         // Comment   

                                                    new ManifestImportConsignorPricing(0,                   // Price
                                                                                       0,                   // Invoice Number
                                                                                       false,               // Verified
                                                                                       false));             // Manually Priced  
                        builderIn.Add(mi); 
                    }
                }
            }

            foreach (var item in builderIn)
            {
                CreateConsignment(item);
            }

            if (_englishImportFailed || _scottishImportFailed)
            {
                var ManifestImportDetailUpdateFailed = new ManifestImportDetailUpdateFailed(_depotNumber.ToString(), DateTime.Now.ToString());
                builderOut.Add(ManifestImportDetailUpdateFailed);
            }

            //TODO - got to process the scottish and english duplicate and imported counts and the file failed markers if present.
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.UpdateAutomatedManifestImportDetailAfterImport", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@depotNumber", _depotNumber);
                    cmd.Parameters.AddWithValue("@EnglishManifestLoadTimestamp", DateTime.Now);
                    cmd.Parameters.AddWithValue("@EnglishCount", _englishCount);
                    cmd.Parameters.AddWithValue("@EnglishImportedCount", _englishImported);
                    cmd.Parameters.AddWithValue("@EnglishDuplicateCount", _englishDuplicate);
                    cmd.Parameters.AddWithValue("@ScottishManifestLoadTimestamp", DateTime.Now);
                    cmd.Parameters.AddWithValue("ScottishCount", _scottishCount);
                    cmd.Parameters.AddWithValue("@ScottishImportedCount", _scottishImported);
                    cmd.Parameters.AddWithValue("@ScottishDuplicateCount", _scottishDuplicate);
                    cmd.Parameters.AddWithValue("@ImportProgress", _importProgress);
                    cmd.Parameters.AddWithValue("@ImportTime", DateTime.Now);


                    int rowsUpdated = cmd.ExecuteNonQuery();

                    if (rowsUpdated <= 0)
                    {
                        logger.Log(LogLevel.Debug, string.Format("Process failed for UpdateAutomatedManifestImportDetail for {0} {1}", _depotNumber, rowsUpdated), "_ManifestImport"); 
                    }
                }
                catch (InvalidOperationException ex)
                {

                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builderOut.Clear();
                }
                catch (SqlException ex)
                {

                    queryStatus = QueryStatus.FAIL;
                    builderOut.Clear();
                }
            }


            //TODO - build the list of cons that failed import 
            return new RetrieveResults<ManifestImportDetailUpdateFailed>(builderOut.ToImmutableList(), queryStatus);
        }

        /// <summary>
        /// Purpose of this is to convert the String ROUTE from the manifest Import JSON into a Int which on passing to the inserting sproc we will convert to the
        /// string representation that is stored on the database; SCOTTish => SCOT and CENTRAL => APC 
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        private int GetRouteID(string route)
        {
            switch (route)
            {
                //case 1: route = "LOCAL"; break;
                //case 2: route = "APC"; break;
                //case 3: route = "SCOT"; break;
                //case 4: route = "NORTH"; break;
                //default: route = "OTHER"; break;
                case "LOCAL": return 1;
                case "CENTRAL": return 2;
                case "SCOTTISH": return 3;
                case "NORTH": return 4;
                default: return 5;
            }
        }

        private DateTime GetSendDate(string dbfDate)
        {
            return (new DateTime(Convert.ToInt32(dbfDate.Substring(0, 4)), Convert.ToInt32(dbfDate.Substring(4, 2)), Convert.ToInt32(dbfDate.Substring(6, 2)), 00, 00, 00));
        }

        private Guid GetImportManifestDepotKey()
        {
            var queryStatus = QueryStatus.OK;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.GetDepotOrganisationNameByDepotNumber", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DepotNumber", _depotNumber);

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        var depotKey = new Guid(Parse.ParseString(dr["DepotKey"].ToString()));
                    }
                }
                catch (InvalidOperationException ex)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                }
                catch (SqlException ex)
                {
                    queryStatus = QueryStatus.FAIL;
                }
            }

            return _depotKey;
        }

        private void CreateConsignment(ManifestImport item)
        {
            // Set Import status for Depot
            ManifestImportProgress status = ManifestImportProgress.ReadyToImport;

            var returnedText = "Started importing for depot " + _depotNumber;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    if (_scottishManifest)
                    {
                        //=========================================================================================================================
                        // this is the equivalent that is in the WCF - we need to make the same updates here 
                        // CREATE PROCEDURE [dbo].[UpdateAutomatedManifestImportDetail].... or similar is required
                        //=========================================================================================================================
                        status = ManifestImportProgress.NewFile;
                        returnedText = ProcessManifestImportItem(item, true);
                        if (returnedText.StartsWith("Imported:")) { _scottishImported++; }
                        else if (returnedText.StartsWith("Failed:")) { _scottishImportFailed = true; }
                        else if (returnedText.StartsWith("Duplicate:")) { _scottishDuplicate++; }
                    }
                    else
                    {
                        //=========================================================================================================================
                        // these are the equivalent that is in the WCF - we need to make the same updates here 
                        // CREATE PROCEDURE [dbo].[UpdateAutomatedManifestImportDetail].... or similar is required
                        //=========================================================================================================================
                        status = ManifestImportProgress.NewFile;
                        if (_depotNumber == 70)
                        {
                            returnedText = ProcessManifest70ImportItem(item, false);
                        }
                        else
                        {
                            returnedText = ProcessManifestImportItem(item, false);
                        }

                        if (returnedText.StartsWith("Imported:")) { _englishImported++; }
                        else if (returnedText.StartsWith("Failed:")) { _englishImportFailed = true; }
                        else if (returnedText.StartsWith("Duplicate:")) { _englishDuplicate++; }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    logger.Log(LogLevel.Error, ex.Message, ex);
                }
                catch (SqlException ex)
                {
                    logger.Log(LogLevel.Error, ex.Message, ex);
                }
            }
        }

        //====================================================================================================================================
        //
        // Note that there are differences between the methods ProcessManifestImportItem and ProcessManifest70ImportItem
        // These are very small differences in only that for the D70 version does not have the following fields set up 
        //
        // cons.ConeeQAS = item.coneeqas;
        // cons.TrueWeight = item.trueweight;
        // cons.DepEntryType = item.entrytype;   
        // 
        // The two methods have been taken from the Continuus WCF and therefore replicate what was in there for imports.  I do not know the 
        // reason for this as the people who wrote it did not document it and have now left
        //
        //====================================================================================================================================
        private string ProcessManifest70ImportItem(ManifestImport item, bool isScottish)
        {
            string route = string.Empty;
            bool log = true;
            string returnedText = "Imported: 0";

            using (SqlConnection connection = new SqlConnection(_connectionString_HubData_MySQL))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.InsertOrUpdateImportedCons", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConNumber", item.conHeader.ConNumber);
                    cmd.Parameters.AddWithValue("@SendDate", item.conHeader.SendDate);
                    cmd.Parameters.AddWithValue("@Accountno", item.conHeader.AccountNo);
                    //cmd.Parameters.AddWithValue("@Contact", item.conCollContact.ContactName);
                    cmd.Parameters.AddWithValue("@ReqDepot", (int)item.conHeader.ReqDepot);
                    cmd.Parameters.AddWithValue("@SendDepot", (int)item.conHeader.SendDepot);
                    cmd.Parameters.AddWithValue("@DelivDepot", (int)item.conHeader.DelivDepot);
                    cmd.Parameters.AddWithValue("@DiscrepCode", item.conHeader.DiscrepancyCode);
                    cmd.Parameters.AddWithValue("@ConeeFao", item.conHeader.ConeeFao);

                    // Collection Address - Consignor
                    cmd.Parameters.AddWithValue("@ConorCompanyName", item.conCollAddress.CompanyName);
                    cmd.Parameters.AddWithValue("@ConorAddressLine1", item.conCollAddress.AddressLine1);
                    cmd.Parameters.AddWithValue("@ConorAddressLine2", item.conCollAddress.AddressLine2);
                    cmd.Parameters.AddWithValue("@ConorTown", item.conCollAddress.Town);
                    cmd.Parameters.AddWithValue("@ConorCounty", item.conCollAddress.County);
                    cmd.Parameters.AddWithValue("@ConorPcode", item.conCollAddress.PostCode);
                    cmd.Parameters.AddWithValue("@ConorContactName", "Unknown");
                    cmd.Parameters.AddWithValue("@ConorContactTelephone", item.conCollContact.ContactTelephone);
                    cmd.Parameters.AddWithValue("@ConorRef", item.conHeader.ConorRef);

                    // Delivery Address - Consignee
                    cmd.Parameters.AddWithValue("@ConeeCompanyName", item.conDelAddress.CompanyName);
                    cmd.Parameters.AddWithValue("@ConeeAddressLine1", item.conDelAddress.AddressLine1);
                    cmd.Parameters.AddWithValue("@ConeeAddressLine2", item.conDelAddress.AddressLine2);
                    cmd.Parameters.AddWithValue("@ConeeTown", item.conDelAddress.Town);
                    cmd.Parameters.AddWithValue("@ConeeCounty", item.conDelAddress.County);
                    cmd.Parameters.AddWithValue("@ConeePcode", item.conDelAddress.PostCode);
                    cmd.Parameters.AddWithValue("@ConeeContactName", item.conDelContact.ContactName);
                    cmd.Parameters.AddWithValue("@ConeeContactTelephone", item.conDelContact.ContactTelephone);

                    cmd.Parameters.AddWithValue("@Service", item.conHeader.Service);
                    cmd.Parameters.AddWithValue("@Cartons", (int)item.conItem.ItemNumber);
                    cmd.Parameters.AddWithValue("@Weight", item.conHeader.Weight);
                    cmd.Parameters.AddWithValue("@IsVolumetric", item.conHeader.IsVolumetric);
                    cmd.Parameters.AddWithValue("@Security", item.conHeader.Security);
                    cmd.Parameters.AddWithValue("@LiabilityValue", Convert.ToInt32(item.conHeader.LiabilityValue));
                    cmd.Parameters.AddWithValue("@SpecialInstructions", item.conHeader.SpecialInstructions);

                    switch (item.conHeader.RouteId)
                    {
                        case 1: route = "LOCAL"; break;
                        case 2: route = "APC"; break;
                        case 3: route = "SCOT"; break;
                        case 4: route = "NORTH"; break;
                        default: route = "OTHER"; break;
                    }
                    if (route != string.Empty) cmd.Parameters.AddWithValue("@Route", route);

                    cmd.Parameters.AddWithValue("@Identifier", item.conHeader.Identifier);
                    cmd.Parameters.AddWithValue("@Fragile", item.conHeader.Fragile);


                    cmd.Parameters.AddWithValue("@LoadedBy", _loadedBy);
                    cmd.Parameters.Add("@LoadedDate", SqlDbType.DateTime);
                    cmd.Parameters["@LoadedDate"].Value = DateTime.Now;
                    cmd.Parameters.Add("@LoadedTime", SqlDbType.Time);
                    cmd.Parameters["@LoadedTime"].Value = DateTime.Now.ToString("hh:mm:ss");

                    //int rowsUpdated = cmd.ExecuteNonQuery();

                    // Create our own object so we can do some conversions
                    //connection.Open();
                    SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM dbo.consignments WHERE identifier = " + item.conHeader.Identifier +
                                                     " AND requestdep = " + item.conHeader.ReqDepot +
                                                     " AND sendDep = " + item.conHeader.SendDepot +
                                                     " AND deliverdep = " + item.conHeader.DelivDepot +
                                                     " AND date = " + item.conHeader.SendDate
                                                    , connection);
                    int conExists = (int)comm.ExecuteScalar();

                    if (conExists == 0)
                    {
                        cmd.Parameters.AddWithValue("@InsertOrUpdate", Enums.InsertOrUpdate.insert.ToString());
                        //TODO call the sproc
                        string message = string.Format("Insert new consignment result {0} for identifier {1}", conExists, item.conHeader.Identifier);
                        logger.Log(LogLevel.Debug, string.Format("Process for {0} {1}", _depotNumber, message), "_ManifestImport");
                    }
                    else
                    {
                        comm = new SqlCommand("SELECT TOP 1 * FROM dbo.consignments WHERE identifier = " + item.conHeader.Identifier +
                                                     " AND requestdep = " + item.conHeader.ReqDepot +
                                                     " AND sendDep = " + item.conHeader.SendDepot +
                                                     " AND deliverdep = " + item.conHeader.DelivDepot +
                                                     " AND date = " + item.conHeader.SendDate
                                                    , connection);
                        var reader = comm.ExecuteReader();
                        DateTime existingDBDate = new DateTime();
                        var existingDBRoute = string.Empty;
                        if (reader.Read())
                        {
                            existingDBDate = Convert.ToDateTime(reader["Date"].ToString());

                        }

                        string identifier = item.conHeader.Identifier;
                        cmd = new SqlCommand("[hubdata_MySQL].[dbo].[InsertOrUpdateImportedCons]", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 5000;

                        //  Check route is local in existing one (database)
                        //  Update the database copy END
                        if (existingDBRoute == "LOCAL" || existingDBRoute == "SCOT" || isScottish)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", Enums.InsertOrUpdate.update.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            string message = string.Format("Route=LOCAL OR SCOT OR IsScottish Update consignment result {0} for identifier {1}", retVal, identifier);
                            logger.Log(LogLevel.Debug, message + "_ManifestImport", log);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        // New REC
                        //  Check if date is different                            
                        //  If different insert dbf->database END
                        else if (item.conHeader.SendDate != existingDBDate)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", Enums.InsertOrUpdate.insert.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            string message = string.Format("Date is same Insert consignment result {0} for identifier {1}", retVal, identifier);
                            logger.Log(LogLevel.Debug, message + "_ManifestImport", log);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        else
                        {
                            // Duplicate REC
                            string message = string.Format("Import has neither inserted or updated consignment table for identifier {0}", item.conHeader.Identifier);
                            logger.Log(LogLevel.Debug, message + "_ManifestImport", log);
                            returnedText = "Duplicate: " + item.conHeader.ConNumber;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Swallow and move on to the next item
                    logger.Log(LogLevel.Debug, string.Format("Exception Processing Manifest { 0} { 1} { 2}", item.conHeader.ConNumber, ex.Message, ex.StackTrace), "_ManifestImport");
                    logger.Log(LogLevel.Debug, "Processing next consignment...", "_ManifestImport", log);
                    var ErrorInfo = "Exception on import " + ex.Message + "\n";
                    returnedText = "Failed: " + ErrorInfo;
                }
                finally { }
            }
            return returnedText;
        }

        //====================================================================================================================================
        //
        // Note that there are differences between the methods ProcessManifestImportItem and ProcessManifest70ImportItem
        // These are very small differences in only that for the D70 version does not have the following fields set up 
        //
        // cons.ConeeQAS = item.coneeqas;
        // cons.TrueWeight = item.trueweight;
        // cons.DepEntryType = item.entrytype;   
        // 
        // The two methods have been taken from the Continuus WCF and therefore replicate what was in there for imports.  I do not know the 
        // reason for this as the people who wrote it did not document it and have now left
        //
        //====================================================================================================================================
        private string ProcessManifestImportItem(ManifestImport item, bool isScottish)
        {
            string route = string.Empty;
            bool log = true;
            string returnedText = "Imported: 0";

            // TODO - we don't know who has done the loading yet - needs adding
            //                string loadedBy = UserMaintenanceProcessing.RetrieveUserNameForUserKey(APCCommon.GetUserKey(sessionKey));
            //                if (loadedBy.Length > 15) loadedBy = loadedBy.Substring(0, 15);
            var userKey = Guid.Parse("2E2F5279-A6D3-46EB-BFD4-78C6A1B51BA0"); 
            using (SqlConnection connection = new SqlConnection(_connectionString_HubData_MySQL))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.InsertOrUpdateImportedCons", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConNumber", item.conHeader.ConNumber);
                    cmd.Parameters.AddWithValue("@SendDate", item.conHeader.SendDate);
                    cmd.Parameters.AddWithValue("@Accountno", item.conHeader.AccountNo);
                    //cmd.Parameters.AddWithValue("@Contact", item.conCollContact.ContactName);
                    cmd.Parameters.AddWithValue("@ReqDepot", (int)item.conHeader.ReqDepot);
                    cmd.Parameters.AddWithValue("@SendDepot", (int)item.conHeader.SendDepot);
                    cmd.Parameters.AddWithValue("@DelivDepot", (int)item.conHeader.DelivDepot);
                    cmd.Parameters.AddWithValue("@DiscrepCode", item.conHeader.DiscrepancyCode);
                    cmd.Parameters.AddWithValue("@ConeeFao", item.conHeader.ConeeFao); 

                    // Collection Address - Consignor
                    cmd.Parameters.AddWithValue("@ConorCompanyName", item.conCollAddress.CompanyName);
                    cmd.Parameters.AddWithValue("@ConorAddressLine1", item.conCollAddress.AddressLine1);
                    cmd.Parameters.AddWithValue("@ConorAddressLine2", item.conCollAddress.AddressLine2);
                    cmd.Parameters.AddWithValue("@ConorTown", item.conCollAddress.Town);
                    cmd.Parameters.AddWithValue("@ConorCounty", item.conCollAddress.County);
                    cmd.Parameters.AddWithValue("@ConorPcode", item.conCollAddress.PostCode);
                    cmd.Parameters.AddWithValue("@ConorContactName", "Unknown");
                    cmd.Parameters.AddWithValue("@ConorContactTelephone", item.conCollContact.ContactTelephone);
                    cmd.Parameters.AddWithValue("@ConorRef", item.conHeader.ConorRef);

                    // Delivery Address - Consignee
                    cmd.Parameters.AddWithValue("@ConeeCompanyName", item.conDelAddress.CompanyName);
                    cmd.Parameters.AddWithValue("@ConeeAddressLine1", item.conDelAddress.AddressLine1);
                    cmd.Parameters.AddWithValue("@ConeeAddressLine2", item.conDelAddress.AddressLine2);
                    cmd.Parameters.AddWithValue("@ConeeTown", item.conDelAddress.Town);
                    cmd.Parameters.AddWithValue("@ConeeCounty", item.conDelAddress.County);
                    cmd.Parameters.AddWithValue("@ConeePcode", item.conDelAddress.PostCode);
                    cmd.Parameters.AddWithValue("@ConeeContactName", item.conDelContact.ContactName);
                    cmd.Parameters.AddWithValue("@ConeeContactTelephone", item.conDelContact.ContactTelephone);

                    cmd.Parameters.AddWithValue("@Service", item.conHeader.Service);
                    cmd.Parameters.AddWithValue("@Cartons", (int)item.conItem.ItemNumber);
                    cmd.Parameters.AddWithValue("@Weight", item.conHeader.Weight);
                    cmd.Parameters.AddWithValue("@IsVolumetric", item.conHeader.IsVolumetric);
                    cmd.Parameters.AddWithValue("@Security", item.conHeader.Security);
                    cmd.Parameters.AddWithValue("@LiabilityValue", Convert.ToInt32(item.conHeader.LiabilityValue));
                    cmd.Parameters.AddWithValue("@SpecialInstructions", item.conHeader.SpecialInstructions);

                    switch (item.conHeader.RouteId)
                    {
                        case 1: route = "LOCAL"; break;
                        case 2: route = "APC"; break;
                        case 3: route = "SCOT"; break;
                        case 4: route = "NORTH"; break;
                        default: route = "OTHER"; break;
                    }
                    if (route != string.Empty) cmd.Parameters.AddWithValue("@Route", route);

                    cmd.Parameters.AddWithValue("@Identifier", item.conHeader.Identifier);
                    cmd.Parameters.AddWithValue("@ConeeQAS", item.conCollAddress.ConeeQas);
                    cmd.Parameters.AddWithValue("@TrueWeight", item.conHeader.TrueWeight);
                    cmd.Parameters.AddWithValue("@Fragile", item.conHeader.Fragile);
                    cmd.Parameters.AddWithValue("@DepEntType", item.conHeader.EntryType);

                    cmd.Parameters.AddWithValue("@LoadedBy", _loadedBy);
                    cmd.Parameters.Add("@LoadedDate", SqlDbType.DateTime);
                    cmd.Parameters["@LoadedDate"].Value = DateTime.Now;
                    cmd.Parameters.Add("@LoadedTime", SqlDbType.Time);
                    cmd.Parameters["@LoadedTime"].Value = DateTime.Now.ToString("hh:mm:ss");

                    //int rowsUpdated = cmd.ExecuteNonQuery();

                    //// Create our own object so we can do some conversions
                    //connection.Open();
                    SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM dbo.consignments WHERE identifier = '" + item.conHeader.Identifier + "'" +
                                                     " AND requestdep = " + item.conHeader.ReqDepot + 
                                                     " AND sendDep = " + item.conHeader.SendDepot +
                                                     " AND deliverdep = " + item.conHeader.DelivDepot +
                                                     " AND date = '" + item.conHeader.SendDate + "'"
                                                    , connection);
                    int conExists = (int)comm.ExecuteScalar();

                    if (conExists == 0)
                    {
                        cmd.Parameters.AddWithValue("@InsertOrUpdate", Enums.InsertOrUpdate.insert.ToString());
                        var retVal = cmd.ExecuteNonQuery();
                        string message = string.Format("Insert new consignment result {0} for identifier {1}", conExists, item.conHeader.Identifier);
                        logger.Log(LogLevel.Debug, string.Format("Process for {0} {1}", _depotNumber, message), "_ManifestImport");
                        returnedText = "Imported: " + item.conHeader.ConNumber;
                    }
                    else
                    {
                        comm = new SqlCommand("SELECT TOP 1 * FROM dbo.consignments WHERE identifier = " + item.conHeader.Identifier +
                                                     " AND requestdep = " + item.conHeader.ReqDepot +
                                                     " AND sendDep = " + item.conHeader.SendDepot +
                                                     " AND deliverdep = " + item.conHeader.DelivDepot +
                                                     " AND date = " + item.conHeader.SendDate
                                                    , connection);
                        var reader = comm.ExecuteReader();
                        DateTime existingDBDate = new DateTime();
                        var existingDBRoute = string.Empty;
                        if (reader.Read())
                        {
                            existingDBDate = Convert.ToDateTime(reader["Date"].ToString());

                        }

                        string identifier = item.conHeader.Identifier;
                        cmd = new SqlCommand("[hubdata_MySQL].[dbo].[InsertOrUpdateImportedCons]", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandTimeout = 5000;

                        //  Check route is local in existing one (database)
                        //  Update the database copy END
                        if (existingDBRoute == "LOCAL" || existingDBRoute == "SCOT" || isScottish)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", Enums.InsertOrUpdate.update.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            string message = string.Format("Route=LOCAL OR SCOT OR IsScottish Update consignment result {0} for identifier {1}", retVal, identifier);
                            logger.Log(LogLevel.Debug, message + "_ManifestImport", log);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        // New REC
                        //  Check if date is different                            
                        //  If different insert dbf->database END
                        else if (item.conHeader.SendDate != existingDBDate)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", Enums.InsertOrUpdate.insert.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            string message = string.Format("Date is same Insert consignment result {0} for identifier {1}", retVal, identifier);
                            logger.Log(LogLevel.Debug, message + "_ManifestImport", log);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        else
                        {
                            // Duplicate REC
                            string message = string.Format("Import has neither inserted or updated consignment table for identifier {0}", item.conHeader.Identifier);
                            logger.Log(LogLevel.Debug, message + "_ManifestImport", log);
                            returnedText = "Duplicate: " + item.conHeader.ConNumber;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Swallow and move on to the next item
                    logger.Log(LogLevel.Debug, string.Format("Exception Processing Manifest { 0} { 1} { 2}", item.conHeader.ConNumber, ex.Message, ex.StackTrace), "_ManifestImport");
                    logger.Log(LogLevel.Debug, "Processing next consignment...", "_ManifestImport", log);
                    var ErrorInfo = "Exception on import " + ex.Message + "\n";
                    returnedText = "Failed: " + ErrorInfo;
                }
                finally { }
            }
            return returnedText;
        }

    }
}