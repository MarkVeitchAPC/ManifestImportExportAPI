using System;
using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Models.ManifestImportStructures;
using System.Collections.Immutable;
using Newtonsoft.Json.Linq;
using System.Linq;
using ManifestImportExportAPI.Enums;
using System.Data.SqlClient;
using System.Data;
using NLog;

namespace ManifestImportExportAPI.Repositories
{
    public class ImportManifestRepository : IImportManifestRepository
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string _connectionStringHubDataMySql;
        private readonly string _connectionString;
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


        public ImportManifestRepository(Guid depotKey)
        {
            _depotKey = depotKey;
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["Main.ConnectionString"];
            _connectionStringHubDataMySql = System.Configuration.ConfigurationManager.AppSettings["connectionString_HubData_MySQL"];
        }

        public RetrieveResults<ManifestImportDetailUpdateFailed> ImportManifest(JObject json)
        {
            var builderOut = ImmutableList.CreateBuilder<ManifestImportDetailUpdateFailed>();
            var builderIn = ImmutableList.CreateBuilder<ManifestImport>();

            var queryStatus = QueryStatus.OK;
            var manifest = new ManifestImportFailed();

            foreach (var item in json)
            {
                if (item.Key != "parameters") continue;
                var param = JArray.Parse(item.Value.ToString());
                foreach (var field in param.Children())
                {
                    var itemProperties = field.Children<JProperty>();
                    _depotNumber = (int)itemProperties.FirstOrDefault(x => x.Name == "depotNumber");
                    _scottishManifest = (bool)itemProperties.FirstOrDefault(x => x.Name == "isScottish");
                    _loadedBy = (string)itemProperties.FirstOrDefault(x => x.Name == "loadedBy");
                }
            }

            foreach (var item in json)
            {
                if (item.Key != "data") continue;
                var data = JArray.Parse(item.Value.ToString());
                foreach (var field in data.Children())
                {
                    var itemProperties = field.Children<JProperty>();

                    var firstOrDefault = itemProperties.FirstOrDefault(x => x.Name == "DATE");
                    if (firstOrDefault == null) continue;
                    {
                        var mi = new ManifestImport(_depotNumber, _scottishManifest,
                            new ManifestImportConsignmentHeader(
                                (string) itemProperties.FirstOrDefault(x => x.Name == "DOCNUM"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "OLDACCNO"),
                                (int) itemProperties.FirstOrDefault(x => x.Name == "REQUESTDEP"),
                                (int) itemProperties.FirstOrDefault(x => x.Name == "SENDDEP"),
                                (int) itemProperties.FirstOrDefault(x => x.Name == "DELIVERDEP"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "SERVICE"),
                                //(DateTime)itemProperties.FirstOrDefault(x => x.Name == "COLLDATE"),
                                DateTime.MinValue, // CollDate
                                GetSendDate(firstOrDefault.Value.ToString()),
                                //DateTime.ParseExact((itemProperties.FirstOrDefault(x => x.Name == "DATE").ToString()), "MM/dd/yyyy hh:mm:ss ttt", CultureInfo.InvariantCulture), 
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORREF"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "IDENTIFIER"),
                                GetRouteID(itemProperties.FirstOrDefault(x => x.Name == "ROUTE").Value.ToString()),
                                null, // ClaimsRef
                                //(bool)itemProperties.FirstOrDefault(x => x.Name == "SECURITY"),
                                (itemProperties.FirstOrDefault(x => x.Name == "SECURITY")).ToString() == "1"
                                    ? true
                                    : false,
                                0, // GoodsTypeId
                                (string) itemProperties.FirstOrDefault(x => x.Name == "SPECIALINS"),
                                DateTime.MinValue, // GoodsReadyTimeFrom
                                DateTime.MinValue, // GoodsReadyTimeTo
                                (Decimal) itemProperties.FirstOrDefault(x => x.Name == "INSURVALUE"),
                                //(bool)itemProperties.FirstOrDefault(x => x.Name == "FRAGILE"),
                                (itemProperties.FirstOrDefault(x => x.Name == "FRAGILE")).ToString() == "1"
                                    ? true
                                    : false,
                                false, // NonConveyorable
                                //(bool)itemProperties.FirstOrDefault(x => x.Name == "VOLUME"),
                                (itemProperties.FirstOrDefault(x => x.Name == "VOLUME")).ToString() == "1"
                                    ? true
                                    : false,
                                (Decimal) itemProperties.FirstOrDefault(x => x.Name == "WEIGHT"),
                                (Decimal) itemProperties.FirstOrDefault(x => x.Name == "TRUEWEIGHT"),
                                string.Empty, // DiscrepCode
                                null, // ConneeRef
                                (string) itemProperties.FirstOrDefault(x => x.Name == "ENTRYTYPE"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEEFAO"),
                                (int) itemProperties.FirstOrDefault(x => x.Name == "CARTONS")),
                            new ManifestImportAddress(
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONSIGNOR"),
                                null, null, null, null,
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORADDR1"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORADDR2"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORADDR3"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORADDR4"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORPCODE"),
                                0, false,
                                //(Boolean)itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")),
                                (itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")).ToString() == "1"
                                    ? true
                                    : false),
                            new ManifestImportAddress(
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONSIGNOR"),
                                null, null, null, null,
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR1"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR2"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR3"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEEADDR4"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEEPCODE"),
                                0, false,
                                //(Boolean)itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")),
                                (itemProperties.FirstOrDefault(x => x.Name == "CONEEQAS")).ToString() == "1"
                                    ? true
                                    : false),
                            new ManifestImportContact(
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONTACT"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONORTEL"),
                                (string) string.Empty, // Mobile Number
                                (string) string.Empty, // Fax Number 
                                (string) string.Empty, // Email Address
                                false, // Email alert
                                false), // Active 

                            new ManifestImportContact(
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONSIGNEE"),
                                (string) itemProperties.FirstOrDefault(x => x.Name == "CONEETEL"),
                                string.Empty, // Mobile Number
                                string.Empty, // Fax Number 
                                string.Empty, // Email Address
                                false, // Email alert
                                false), // Active 

                            new ManifestImportConsignmentItem(
                                0, // ItemStatus 
                                0, // Item Number
                                //(bool)itemProperties.FirstOrDefault(x => x.Name == "VOLUME"),
                                (itemProperties.FirstOrDefault(x => x.Name == "VOLUME")).ToString() == "1" ? true : false,
                                string.Empty, // ShortRef
                                string.Empty, // CustomerRef1
                                string.Empty, // CustomerRef2 
                                false, // AutuPrintedLabel
                                false, // LabelPrinted
                                (decimal) itemProperties.FirstOrDefault(x => x.Name == "WEIGHT"),
                                (decimal) itemProperties.FirstOrDefault(x => x.Name == "LSIZE"),
                                (decimal) itemProperties.FirstOrDefault(x => x.Name == "WSIZE"),
                                (decimal) itemProperties.FirstOrDefault(x => x.Name == "HSIZE"),
                                (decimal) itemProperties.FirstOrDefault(x => x.Name == "TRUEWEIGHT")),
                            new ManifestImportComment
                                (0, // CommentVisibityId
                                    string.Empty), // Comment   

                            new ManifestImportConsignorPricing
                                (0, // Price
                                    0, // Invoice Number
                                    false, // Verified
                                    false)); // Manually Priced  
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
                var manifestImportDetailUpdateFailed = new ManifestImportDetailUpdateFailed(_depotNumber.ToString(), DateTime.Now.ToString());
                builderOut.Add(manifestImportDetailUpdateFailed);
            }

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.UpdateAutomatedManifestImportDetailAfterImport", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
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


                    var rowsUpdated = cmd.ExecuteNonQuery();

                    if (rowsUpdated <= 0)
                    {
                        logger.Log(LogLevel.Debug, string.Format("Process failed for UpdateAutomatedManifestImportDetail for {0} {1}", _depotNumber, rowsUpdated), "_ManifestImport"); 
                    }
                }
                catch (InvalidOperationException)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builderOut.Clear();
                }
                catch (SqlException)
                {
                    queryStatus = QueryStatus.FAIL;
                    builderOut.Clear();
                }
            }

            return new RetrieveResults<ManifestImportDetailUpdateFailed>(builderOut.ToImmutableList(), queryStatus);
        }

        /// <summary>
        /// Purpose of this is to convert the String ROUTE from the manifest Import JSON into a Int which on passing to the inserting sproc we will convert to the
        /// string representation that is stored on the database; SCOTTish => SCOT and CENTRAL => APC 
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        private static int GetRouteID(string route)
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

        private static DateTime GetSendDate(string dbfDate)
        {
            return (new DateTime(Convert.ToInt32(dbfDate.Substring(0, 4)), Convert.ToInt32(dbfDate.Substring(4, 2)), Convert.ToInt32(dbfDate.Substring(6, 2)), 00, 00, 00));
        }

        private void CreateConsignment(ManifestImport item)
        {
            try
            {
                string returnedText;
                if (_scottishManifest)
                {
                    //=========================================================================================================================
                    // this is the equivalent that is in the WCF - we need to make the same updates here 
                    // CREATE PROCEDURE [dbo].[UpdateAutomatedManifestImportDetail].... or similar is required
                    //=========================================================================================================================
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
                    returnedText = _depotNumber == 70 ? ProcessManifest70ImportItem(item, false) : ProcessManifestImportItem(item, false);

                    if (returnedText.StartsWith("Imported:")) { _englishImported++; }
                    else if (returnedText.StartsWith("Failed:")) {_englishImportFailed = true; }
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
            var route = string.Empty;
            var log = true;
            var returnedText = "Imported: 0";

            using (SqlConnection connection = new SqlConnection(_connectionStringHubDataMySql))
            {
                var existingDbDate = new DateTime();
                var existingDbRoute = string.Empty;
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.InsertOrUpdateImportedCons", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    AssignConsignmentHeaderParameters(item, cmd);
                    AssignConsignorAddressParameters(item, cmd);
                    AssignConsigneeAddressParameters(item, cmd);
                    AssignConsignmentItemParameters(item, cmd);
                    AssignRouteParameter(item, cmd);

                    AssignLoadedDetailsParameters(cmd);

                    var comm = new SqlCommand(CmdTextForConsignmentCount(item), connection);
                    var conExists = (int)comm.ExecuteScalar();

                    if (conExists == 0)
                    {
                        cmd.Parameters.AddWithValue("@InsertOrUpdate", InsertOrUpdate.insert.ToString());
                        cmd.ExecuteNonQuery();
                        var message = string.Format("Insert new consignment result {0} for identifier {1}", conExists, item.conHeader.Identifier);
                        logger.Log(LogLevel.Debug, string.Format("Process for {0} {1}", _depotNumber, message), "_ManifestImport");
                    }
                    else
                    {
                        comm = new SqlCommand(CmdTextForGetSpecificCon(item), connection);
                        var reader = comm.ExecuteReader();
                        if (reader.Read())
                        {
                            existingDbDate = Convert.ToDateTime(reader["Date"].ToString());
                        }

                        var identifier = item.conHeader.Identifier;
                        cmd = new SqlCommand("[hubdata_MySQL].[dbo].[InsertOrUpdateImportedCons]", connection)
                        {
                            CommandType = CommandType.StoredProcedure,
                            CommandTimeout = 5000
                        };

                        //  Check route is local in existing one (database)
                        //  Update the database copy END
                        if (existingDbRoute == "LOCAL" || existingDbRoute == "SCOT" || isScottish)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", InsertOrUpdate.update.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            logger.Log(LogLevel.Debug, string.Format("Route=LOCAL OR SCOT OR IsScottish Update consignment result {0} for identifier {1}", retVal, identifier) + "_ManifestImport", true);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        // New REC
                        //  Check if date is different                            
                        //  If different insert dbf->database END
                        else if (item.conHeader.SendDate != existingDbDate)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", InsertOrUpdate.insert.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            logger.Log(LogLevel.Debug, string.Format("Date is same Insert consignment result {0} for identifier {1}", retVal, identifier) + "_ManifestImport", true);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        else
                        {
                            // Duplicate REC
                            logger.Log(LogLevel.Debug, string.Format("Import has neither inserted or updated consignment table for identifier {0}", item.conHeader.Identifier) + "_ManifestImport", true);
                            returnedText = "Duplicate: " + item.conHeader.ConNumber;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Swallow and move on to the next item
                    logger.Log(LogLevel.Debug, string.Format("Exception Processing Manifest {0} {1} {2}", item.conHeader.ConNumber, ex.Message, ex.StackTrace), "_ManifestImport");
                    logger.Log(LogLevel.Debug, "Processing next consignment...", "_ManifestImport", true);
                    var errorInfo = "Exception on import " + ex.Message + "\n";
                    returnedText = "Failed: " + errorInfo;
                }
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
            const bool log = true;
            var returnedText = "Imported: 0";

            using (var connection = new SqlConnection(_connectionStringHubDataMySql))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.InsertOrUpdateImportedCons", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };

                    AssignConsignmentHeaderParameters(item, cmd);
                    AssignConsignorAddressParameters(item, cmd);
                    AssignConsigneeAddressParameters(item, cmd);
                    AssignConsignmentItemParameters(item, cmd);
                    AssignRouteParameter(item, cmd);

                    cmd.Parameters.AddWithValue("@ConeeQAS", item.conCollAddress.ConeeQas);
                    cmd.Parameters.AddWithValue("@TrueWeight", item.conHeader.TrueWeight);
                    cmd.Parameters.AddWithValue("@DepEntType", item.conHeader.EntryType);

                    AssignLoadedDetailsParameters(cmd);

                    // Create our own object so we can do some conversions
                    var comm = new SqlCommand(CmdTextForConsignmentCount(item), connection);
                    var conExists = (int)comm.ExecuteScalar();

                    if (conExists == 0)
                    {
                        cmd.Parameters.AddWithValue("@InsertOrUpdate", InsertOrUpdate.insert.ToString());
                        cmd.ExecuteNonQuery();
                        string message = string.Format("Insert new consignment result {0} for identifier {1}", conExists, item.conHeader.Identifier);
                        logger.Log(LogLevel.Debug, string.Format("Process for {0} {1}", _depotNumber, message), "_ManifestImport");
                        returnedText = "Imported: " + item.conHeader.ConNumber;
                    }
                    else
                    {
                        comm = new SqlCommand(CmdTextForGetSpecificCon(item), connection);
                        var reader = comm.ExecuteReader();
                        var existingDbDate = new DateTime();
                        var existingDbRoute = string.Empty;
                        if (reader.Read())
                        {
                            existingDbDate = Convert.ToDateTime(reader["Date"].ToString());
                        }

                        var identifier = item.conHeader.Identifier;
                        cmd = new SqlCommand("[hubdata_MySQL].[dbo].[InsertOrUpdateImportedCons]", connection)
                        {
                            CommandType = CommandType.StoredProcedure,
                            CommandTimeout = 5000
                        };

                        //  Check route is local in existing one (database)
                        //  Update the database copy END
                        if (existingDbRoute == "LOCAL" || existingDbRoute == "SCOT" || isScottish)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", InsertOrUpdate.update.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            logger.Log(LogLevel.Debug, string.Format("Route=LOCAL OR SCOT OR IsScottish Update consignment result {0} for identifier {1}", retVal, identifier) + "_ManifestImport", true);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        // New REC
                        //  Check if date is different                            
                        //  If different insert dbf->database END
                        else if (item.conHeader.SendDate != existingDbDate)
                        {
                            cmd.Parameters.AddWithValue("@InsertOrUpdate", InsertOrUpdate.insert.ToString());
                            var retVal = cmd.ExecuteNonQuery();
                            logger.Log(LogLevel.Debug, string.Format("Date is same Insert consignment result {0} for identifier {1}", retVal, identifier) + "_ManifestImport", true);
                            returnedText = "Imported: " + item.conHeader.ConNumber;
                        }
                        else
                        {
                            // Duplicate REC
                            logger.Log(LogLevel.Debug, string.Format("Import has neither inserted or updated consignment table for identifier {0}", item.conHeader.Identifier) + "_ManifestImport", true);
                            returnedText = "Duplicate: " + item.conHeader.ConNumber;
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Swallow and move on to the next item
                    logger.Log(LogLevel.Debug, string.Format("Exception Processing Manifest {0} {1} {2}", item.conHeader.ConNumber, ex.Message, ex.StackTrace), "_ManifestImport");
                    logger.Log(LogLevel.Debug, "Processing next consignment...", "_ManifestImport", log);
                    var errorInfo = "Exception on import " + ex.Message + "\n";
                    returnedText = "Failed: " + errorInfo;
                }
            }
            return returnedText;
        }

        private static string CmdTextForGetSpecificCon(ManifestImport item)
        {
            return "SELECT TOP 1 * FROM dbo.consignments WHERE identifier = " + item.conHeader.Identifier +
                   " AND requestdep = " + item.conHeader.ReqDepot +
                   " AND sendDep = " + item.conHeader.SendDepot +
                   " AND deliverdep = " + item.conHeader.DelivDepot +
                   " AND date = " + item.conHeader.SendDate;
        }

        private static string CmdTextForConsignmentCount(ManifestImport item)
        {
            return "SELECT COUNT(*) FROM dbo.consignments WHERE identifier = '" + item.conHeader.Identifier + "'" +
                   " AND requestdep = " + item.conHeader.ReqDepot + 
                   " AND sendDep = " + item.conHeader.SendDepot +
                   " AND deliverdep = " + item.conHeader.DelivDepot +
                   " AND date = '" + item.conHeader.SendDate + "'";
        }

        private static void AssignRouteParameter(ManifestImport item, SqlCommand cmd)
        {
            string route;
            switch (item.conHeader.RouteId)
            {
                case 1: route = "LOCAL"; break;
                case 2: route = "APC"; break;
                case 3: route = "SCOT"; break;
                case 4: route = "NORTH"; break;
                default: route = "OTHER"; break;
            }
            if (route != string.Empty) cmd.Parameters.AddWithValue("@Route", route);
        }

        private static void AssignConsignmentItemParameters(ManifestImport item, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@Service", item.conHeader.Service);
            cmd.Parameters.AddWithValue("@Cartons", item.conItem.ItemNumber);
            cmd.Parameters.AddWithValue("@Weight", item.conHeader.Weight);
            cmd.Parameters.AddWithValue("@IsVolumetric", item.conHeader.IsVolumetric);
            cmd.Parameters.AddWithValue("@Security", item.conHeader.Security);
            cmd.Parameters.AddWithValue("@LiabilityValue", Convert.ToInt32(item.conHeader.LiabilityValue));
            cmd.Parameters.AddWithValue("@SpecialInstructions", item.conHeader.SpecialInstructions);
            cmd.Parameters.AddWithValue("@Identifier", item.conHeader.Identifier);
            cmd.Parameters.AddWithValue("@Fragile", item.conHeader.Fragile);
        }

        private static void AssignConsigneeAddressParameters(ManifestImport item, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@ConeeCompanyName", item.conDelAddress.CompanyName);
            cmd.Parameters.AddWithValue("@ConeeAddressLine1", item.conDelAddress.AddressLine1);
            cmd.Parameters.AddWithValue("@ConeeAddressLine2", item.conDelAddress.AddressLine2);
            cmd.Parameters.AddWithValue("@ConeeTown", item.conDelAddress.Town);
            cmd.Parameters.AddWithValue("@ConeeCounty", item.conDelAddress.County);
            cmd.Parameters.AddWithValue("@ConeePcode", item.conDelAddress.PostCode);
            cmd.Parameters.AddWithValue("@ConeeContactName", item.conDelContact.ContactName);
            cmd.Parameters.AddWithValue("@ConeeContactTelephone", item.conDelContact.ContactTelephone);
        }

        private static void AssignConsignorAddressParameters(ManifestImport item, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@ConorCompanyName", item.conCollAddress.CompanyName);
            cmd.Parameters.AddWithValue("@ConorAddressLine1", item.conCollAddress.AddressLine1);
            cmd.Parameters.AddWithValue("@ConorAddressLine2", item.conCollAddress.AddressLine2);
            cmd.Parameters.AddWithValue("@ConorTown", item.conCollAddress.Town);
            cmd.Parameters.AddWithValue("@ConorCounty", item.conCollAddress.County);
            cmd.Parameters.AddWithValue("@ConorPcode", item.conCollAddress.PostCode);
            cmd.Parameters.AddWithValue("@ConorContactName", "Unknown");
            cmd.Parameters.AddWithValue("@ConorContactTelephone", item.conCollContact.ContactTelephone);
            cmd.Parameters.AddWithValue("@ConorRef", item.conHeader.ConorRef);
        }

        private static void AssignConsignmentHeaderParameters(ManifestImport item, SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@ConNumber", item.conHeader.ConNumber);
            cmd.Parameters.AddWithValue("@SendDate", item.conHeader.SendDate);
            cmd.Parameters.AddWithValue("@Accountno", item.conHeader.AccountNo);
            //cmd.Parameters.AddWithValue("@Contact", item.conCollContact.ContactName);
            cmd.Parameters.AddWithValue("@ReqDepot", item.conHeader.ReqDepot);
            cmd.Parameters.AddWithValue("@SendDepot", item.conHeader.SendDepot);
            cmd.Parameters.AddWithValue("@DelivDepot", item.conHeader.DelivDepot);
            cmd.Parameters.AddWithValue("@DiscrepCode", item.conHeader.DiscrepancyCode);
            cmd.Parameters.AddWithValue("@ConeeFao", item.conHeader.ConeeFao);
        }

        private void AssignLoadedDetailsParameters(SqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@LoadedBy", _loadedBy);
            cmd.Parameters.Add("@LoadedDate", SqlDbType.DateTime);
            cmd.Parameters["@LoadedDate"].Value = DateTime.Now;
            cmd.Parameters.Add("@LoadedTime", SqlDbType.Time);
            cmd.Parameters["@LoadedTime"].Value = DateTime.Now.ToString("hh:mm:ss");
        }
    }
}