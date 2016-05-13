using System;
using ManifestImportExportAPI.Models;
using System.Collections.Immutable;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using ManifestImportExportAPI.Enums;
using System.Data.SqlClient;
using System.Data;
using ManifestImportExportAPI.Domain;
using NLog;

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
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
            _connectionString_HubData_MySQL = System.Configuration.ConfigurationManager.AppSettings["connectionString_HubData_MySQL"];
        }

        public RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(string Json, int depotnumber, bool scottishManifest)
        {
            _depotNumber = depotnumber;
            _scottishManifest = scottishManifest;
            _depotKey = GetImportManifestDepotKey(); 

            //var builderOut = ImmutableList.CreateBuilder<List<ManifestImportFailed>>();
            var builderOut = ImmutableList.CreateBuilder<ManifestImportDetailUpdateFailed>();
            var builderIn = ImmutableList.CreateBuilder<ManifestImport>();

            var queryStatus = QueryStatus.OK;
            var lastHeaderKey = string.Empty;

            ManifestImportFailed manifest = new ManifestImportFailed();

            JArray importCons = JArray.Parse(Json);
            var importConsList = importCons.Select(p => new ManifestImport(p));
            foreach (var item in importConsList)
            {
                builderIn.Add(item);
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

            // TODO - we don't know who has done the loading yet - needs adding
            //                string loadedBy = UserMaintenanceProcessing.RetrieveUserNameForUserKey(APCCommon.GetUserKey(sessionKey));
            //                if (loadedBy.Length > 15) loadedBy = loadedBy.Substring(0, 15);
            using (SqlConnection connection = new SqlConnection(_connectionString_HubData_MySQL))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.InsertOrUpdateManifestConsignment", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConNumber", item.conHeader.ConNumber);
                    cmd.Parameters.AddWithValue("@SendDate", item.conHeader.SendDate);
                    cmd.Parameters.AddWithValue("@Accountno", item.conHeader.AccountNo);
                    cmd.Parameters.AddWithValue("@Contact", item.conCollContact.ContactName);
                    cmd.Parameters.AddWithValue("@ReqDepot", (int)item.conHeader.ReqDepot);
                    cmd.Parameters.AddWithValue("@SendDepot", (int)item.conHeader.SendDepot);
                    cmd.Parameters.AddWithValue("@DelivDepot", (int)item.conHeader.DelivDepot);
                    cmd.Parameters.AddWithValue("@DiscrepCode", item.conHeader.DiscrepancyCode);

                    // Collection Address - Consignor
                    cmd.Parameters.AddWithValue("@ConorCompanyName", item.conCollAddress.CompanyName);
                    cmd.Parameters.AddWithValue("@ConorAddressLine1", item.conCollAddress.AddressLine1);
                    cmd.Parameters.AddWithValue("@ConorAddressLine2", item.conCollAddress.AddressLine2);
                    cmd.Parameters.AddWithValue("@ConorTown", item.conCollAddress.Town);
                    cmd.Parameters.AddWithValue("@ConorCounty", item.conCollAddress.County);
                    cmd.Parameters.AddWithValue("@ConorPcode", item.conCollAddress.PostCode);
                    cmd.Parameters.AddWithValue("@ConorContactName", "Unknown");
                    cmd.Parameters.AddWithValue("@ConorContactTelephone", item.conCollContact.ContactTelephone);

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

                    //TODO - add the user key of the person doing the import
                    //cons.LoadedBy = loadedBy;
                    cmd.Parameters.AddWithValue("@LoadedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@LoadedTime", string.Format("{ 0:d2}:{1:d2}:{2:d2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));

                    int rowsUpdated = cmd.ExecuteNonQuery();

                    // Create our own object so we can do some conversions
                    connection.Open();
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
            using (SqlConnection connection = new SqlConnection(_connectionString_HubData_MySQL))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.InsertOrUpdateManifestConsignment", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@ConNumber", item.conHeader.ConNumber);
                    cmd.Parameters.AddWithValue("@SendDate", item.conHeader.SendDate);
                    cmd.Parameters.AddWithValue("@Accountno", item.conHeader.AccountNo);
                    cmd.Parameters.AddWithValue("@Contact", item.conCollContact.ContactName);
                    cmd.Parameters.AddWithValue("@ReqDepot", (int)item.conHeader.ReqDepot);
                    cmd.Parameters.AddWithValue("@SendDepot", (int)item.conHeader.SendDepot);
                    cmd.Parameters.AddWithValue("@DelivDepot", (int)item.conHeader.DelivDepot);
                    cmd.Parameters.AddWithValue("@DiscrepCode", item.conHeader.DiscrepancyCode);

                    // Collection Address - Consignor
                    cmd.Parameters.AddWithValue("@ConorCompanyName", item.conCollAddress.CompanyName);
                    cmd.Parameters.AddWithValue("@ConorAddressLine1", item.conCollAddress.AddressLine1);
                    cmd.Parameters.AddWithValue("@ConorAddressLine2", item.conCollAddress.AddressLine2);
                    cmd.Parameters.AddWithValue("@ConorTown", item.conCollAddress.Town);
                    cmd.Parameters.AddWithValue("@ConorCounty", item.conCollAddress.County);
                    cmd.Parameters.AddWithValue("@ConorPcode", item.conCollAddress.PostCode);
                    cmd.Parameters.AddWithValue("@ConorContactName", "Unknown");
                    cmd.Parameters.AddWithValue("@ConorContactTelephone", item.conCollContact.ContactTelephone);

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

                    //TODO - add the user key of the person doing the import
                    //cons.LoadedBy = loadedBy;
                    cmd.Parameters.AddWithValue("@LoadedDate", DateTime.Now);
                    cmd.Parameters.AddWithValue("@LoadedTime", string.Format("{ 0:d2}:{1:d2}:{2:d2}", DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second));

                    int rowsUpdated = cmd.ExecuteNonQuery();

                    // Create our own object so we can do some conversions
                    connection.Open();
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