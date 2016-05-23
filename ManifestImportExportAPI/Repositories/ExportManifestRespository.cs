using ManifestImportExportAPI.Domain;
using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories.RepositoryInterfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Immutable;
using System.Data;
using System.Data.SqlClient;

namespace ManifestImportExportAPI.Repositories
{
    public class ExportManifestRepository : IExportManifestRepository
    {
        private readonly string _connectionString;

        public ExportManifestRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["Main.ConnectionString"];
        }

        public RetrieveResults<ManifestExport> ExportManifest(Int32 depotNumber, DateTime manifestDate)
        {
            var builder = ImmutableList.CreateBuilder<ManifestExport>();
            var queryStatus = QueryStatus.OK;

            using (var connection1 = new SqlConnection(_connectionString))
            {
                try
                {
                    connection1.Open();
                    var cmd = new SqlCommand("dbo.GetDepotOrganisationNameByDepotNumber", connection1)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@DepotNumber", depotNumber);

                    var dr = cmd.ExecuteReader();

                    if (dr.HasRows)
                    {
                        dr.Read();
                        var depotKey = Parse.ParseString(dr["DepotKey"].ToString());

                        using (var connection2 = new SqlConnection(_connectionString))
                        {
                            try
                            {
                                connection2.Open();
                                var cmd1 = new SqlCommand("dbo.GetConsignmentsForManifestExportFromTNG2", connection2)
                                {
                                    CommandType = CommandType.StoredProcedure
                                };
                                cmd1.Parameters.Add("@ManifestDate", SqlDbType.Date);
                                cmd1.Parameters["@ManifestDate"].Value = manifestDate.Date;
                                cmd1.Parameters.Add("@DepotKey", SqlDbType.UniqueIdentifier);
                                cmd1.Parameters["@DepotKey"].Value = new Guid(depotKey);

                                var dr1 = cmd1.ExecuteReader();

                                while (dr1.Read())
                                {
                                    var manifestExport = ParseManifestExport(dr1);
                                    builder.Add(manifestExport);
                                }
                                if (builder.Count == 0) { queryStatus = QueryStatus.NO_DATA; }
                            }
                            catch (InvalidOperationException)
                            {
                                queryStatus = QueryStatus.FAILED_CONNECTION;
                                builder.Clear();
                            }
                            catch (SqlException)
                            {
                                queryStatus = QueryStatus.FAIL;
                                builder.Clear();
                            }
                        } 
                    }
                }
                catch (InvalidOperationException)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException)
                {
                    queryStatus = QueryStatus.FAIL;
                    builder.Clear();
                }
            }

            return new RetrieveResults<ManifestExport>(builder.ToImmutableList(), queryStatus);
        }

        public RetrieveResults<ManifestExportBarcode> ExportManifestBarcode(int depotNumber, DateTime manifestDate)
        {
            var builder = ImmutableList.CreateBuilder<ManifestExportBarcode>();
            var queryStatus = QueryStatus.OK;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.GetDepotOrganisationNameByDepotNumber", connection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    cmd.Parameters.AddWithValue("@DepotNumber", depotNumber);

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        var depotKey = Parse.ParseString(dr["DepotKey"].ToString());

                        using (var connection1 = new SqlConnection(_connectionString))
                        {
                            try
                            {
                                connection1.Open();

                                var cmd1 = new SqlCommand("dbo.GetBarcodeForManifestExportFromTNG2", connection1)
                                {
                                    CommandType = CommandType.StoredProcedure
                                };
                                cmd1.Parameters.AddWithValue("@ManifestDate", manifestDate);
                                cmd1.Parameters.AddWithValue("@DepotKey", depotKey);

                                var dr1 = cmd1.ExecuteReader();

                                while (dr1.Read())
                                {
                                    var manifestExportBarcode = ParseManifestExportBarcode(dr1);
                                    builder.Add(manifestExportBarcode);
                                }
                                if (builder.Count == 0) { queryStatus = QueryStatus.NO_DATA; }
                            }
                            catch (InvalidOperationException)
                            {
                                queryStatus = QueryStatus.FAILED_CONNECTION;
                                builder.Clear();
                            }
                            catch (SqlException)
                            {
                                queryStatus = QueryStatus.FAIL;
                                builder.Clear();
                            }
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException)
                {
                    queryStatus = QueryStatus.FAIL;
                    builder.Clear();
                }
            }
            return new RetrieveResults<ManifestExportBarcode>(builder.ToImmutableList(), queryStatus);
        }

        public RetrieveResults<ManifestExportCounts> ManifestExportCounts()
        {
            var builder = ImmutableList.CreateBuilder<ManifestExportCounts>();
            var queryStatus = QueryStatus.OK;

            using (var connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var comm = new SqlCommand("SELECT COUNT(*) FROM dbo.ConsignmentsToBePriced WHERE PricingCompleted IS NULL", connection);
                    var unpricedConsignments = (int)comm.ExecuteScalar();

                    comm = new SqlCommand("SELECT COUNT(*) FROM dbo.ConsignmentsToBeMigrated WHERE Migrated = 0", connection);
                    var unmigratedConsignments = (int)comm.ExecuteScalar();

                    var manifestExportCounts = new ManifestExportCounts(unpricedConsignments, unmigratedConsignments);
                    builder.Add(manifestExportCounts);
                }
                catch (InvalidOperationException)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException)
                {
                    queryStatus = QueryStatus.FAIL;
                    builder.Clear();
                }
            }

            return new RetrieveResults<ManifestExportCounts>(builder.ToImmutableList(), queryStatus);
        }

        // Updates of database called [Continuus].[dbo].[AutomatedManifestImportDetail] which holds import and export summaries.  
        // This is used primarily for updating the LastExportDateTime but maybe expanded later.
        //
        // The JSON object for this should be ....
        // {
        //  "ExportedFiles": [
        //                    {"DepotNumber":"1", "LastExport":"2016-04-10 12:30:00.000"}
        //                    {"DepotNumber":"2", "LastExport":"2016-04-10 12:34:10.000"}
        //                    {"DepotNumber":"3", "LastExport":"2016-04-10 13:30:00.000"}
        //                   ] 
        // }
        public RetrieveResults<ManifestImportDetailUpdateFailed> ManifestImportDetailUpdate(string json)
        {
            var builder = ImmutableList.CreateBuilder<ManifestImportDetailUpdateFailed>();
            var queryStatus = QueryStatus.OK;
            var con = JObject.Parse(json);
            var items = (JArray)con["ExportedFiles"];

            foreach (var t in items)
            {
                var item = (JObject)t;
                var jtoken = item.First;
                var depotNo = string.Empty;
                var exportDateTime = string.Empty;

                while (jtoken != null)
                {
                    switch (((JProperty) jtoken).Name)
                    {
                        case "DepotNumber":
                            depotNo = ((JProperty) jtoken).Value.ToString();
                            break;
                        case "LastExport":
                            exportDateTime = ((JProperty) jtoken).Value.ToString();
                            break;
                    }
                    jtoken = jtoken.Next;
                }

                // retrieved the depot and export datetime from the Json item record - call the update sproc 
                using (var connection = new SqlConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                        var cmd = new SqlCommand("dbo.UpdateAutomatedManifestImportDetail", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        cmd.Parameters.AddWithValue("@depotNumber", depotNo);
                        cmd.Parameters.AddWithValue("@lastExportDate", exportDateTime);

                        var rowsUpdated = cmd.ExecuteNonQuery();

                        if (rowsUpdated <= 0)
                        {
                            var manifestImportDetailUpdateFailed = new ManifestImportDetailUpdateFailed(depotNo, exportDateTime);
                            builder.Add(manifestImportDetailUpdateFailed);
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        queryStatus = QueryStatus.FAILED_CONNECTION;
                        builder.Clear();
                    }
                    catch (SqlException)
                    {
                        queryStatus = QueryStatus.FAIL;
                        builder.Clear();
                    }
                }
            }
            return new RetrieveResults<ManifestImportDetailUpdateFailed>(builder.ToImmutableList(), queryStatus);
        }

        private ManifestExport ParseManifestExport(SqlDataReader dr)
        {
            var docnum = Parse.ParseString(dr["docnum"].ToString());
            var date = Parse.ParseString(dr["date"].ToString());
            var accountno = Parse.ParseString(dr["accountno"].ToString());
            var contact = Parse.ParseString(dr["contact"].ToString());
            var requestdep = Parse.ParseString(dr["requestdep"].ToString());
            var senddep = Parse.ParseString(dr["senddep"].ToString());
            var deliverdep = Parse.ParseString(dr["deliverdep"].ToString());
            var consignor = Parse.ParseString(dr["consignor"].ToString());
            var conortel = Parse.ParseString(dr["conortel"].ToString());
            var conoraddr1 = Parse.ParseString(dr["conoraddr1"].ToString());
            var conoraddr2 = Parse.ParseString(dr["conoraddr2"].ToString());
            var conoraddr3 = Parse.ParseString(dr["conoraddr3"].ToString());
            var conoraddr4 = Parse.ParseString(dr["conoraddr4"].ToString());
            var conorpcode = Parse.ParseString(dr["conorpcode"].ToString());
            var conorref = Parse.ParseString(dr["conorref"].ToString());
            var consignee = Parse.ParseString(dr["consignee"].ToString());
            var coneetel = Parse.ParseString(dr["coneetel"].ToString());
            var coneeaddr1 = Parse.ParseString(dr["coneeaddr1"].ToString());
            var coneeaddr2 = Parse.ParseString(dr["coneeaddr2"].ToString());
            var coneeaddr3 = Parse.ParseString(dr["coneeaddr3"].ToString());
            var coneeaddr4 = Parse.ParseString(dr["coneeaddr4"].ToString());
            var coneepcode = Parse.ParseString(dr["coneepcode"].ToString());
            var coneefao = Parse.ParseString(dr["coneefao"].ToString());
            var service = Parse.ParseString(dr["service"].ToString());
            var cartons = Parse.ParseString(dr["cartons"].ToString());
            var weight = Parse.ParseString(dr["weight"].ToString());
            var volume = Parse.ParseString(dr["volume"].ToString());
            var security = Parse.ParseString(dr["security"].ToString());
            var insurvalue = Parse.ParseString(dr["insurvalue"].ToString());
            var specialins = Parse.ParseString(dr["specialins"].ToString());
            var delivprice = Parse.ParseString(dr["delivprice"].ToString());
            var route = Parse.ParseString(dr["route"].ToString());
            var identifier = Parse.ParseString(dr["identifier"].ToString());
            var mintime = Parse.ParseString(dr["mintime"].ToString());
            var maxtime = Parse.ParseString(dr["maxtime"].ToString());
            var manifestExportRecord = new ManifestExport(docnum, date, accountno, contact, requestdep, senddep, deliverdep, consignor, conortel,
                                                          conoraddr1, conoraddr2, conoraddr3, conoraddr4, conorpcode, conorref, consignee, coneetel,
                                                          coneeaddr1, coneeaddr2, coneeaddr3, coneeaddr4, coneepcode, coneefao, service, cartons, weight,
                                                          volume, security, insurvalue, specialins, delivprice, route, identifier, mintime, maxtime);
            return manifestExportRecord;
        }

        private ManifestExportBarcode ParseManifestExportBarcode(SqlDataReader dr)
        {
            var identifier = Parse.ParseString(dr["identifier"].ToString());
            var barcode = Parse.ParseString(dr["barcode"].ToString());
            var manifestExportBarcodeRecord = new ManifestExportBarcode(identifier, barcode);
            return manifestExportBarcodeRecord;
        }
    }
} 