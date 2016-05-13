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
        private string _connectionString;

        public ExportManifestRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
        }

        public RetrieveResults<ManifestExport> ExportManifest(Int32 depotNumber, DateTime manifestDate)
        {
            var builder = ImmutableList.CreateBuilder<ManifestExport>();
            var queryStatus = QueryStatus.OK;

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.GetDepotOrganisationNameByDepotNumber", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DepotNumber", depotNumber);

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        var depotKey = Parse.ParseString(dr["DepotNumber"].ToString());

                        cmd = new SqlCommand("dbo.GetConsignmentsForManifestExportFromTNG2", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ManifestDate", manifestDate);
                        cmd.Parameters.AddWithValue("@DepotKey", depotKey);

                        dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var manifestExport = ParseManifestExport(dr);
                                builder.Add(manifestExport);
                            }
                        }
                        else
                        {
                            queryStatus = QueryStatus.NO_DATA;
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {

                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException ex)
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

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new SqlCommand("dbo.GetDepotOrganisationNameByDepotNumber", connection);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@DepotNumber", depotNumber);

                    var dr = cmd.ExecuteReader();
                    if (dr.HasRows)
                    {
                        dr.Read();
                        var depotKey = Parse.ParseString(dr["DepotKey"].ToString());

                        cmd = new SqlCommand("dbo.GetBarcodeForManifestExportFromTNG2", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@ManifestDate", manifestDate);
                        cmd.Parameters.AddWithValue("@DepotKey", depotKey);

                        dr = cmd.ExecuteReader();
                        if (dr.HasRows)
                        {
                            while (dr.Read())
                            {
                                var manifestExportBarcode = ParseManifestExportBarcode(dr);
                                builder.Add(manifestExportBarcode);
                            }
                        }
                        else
                        {
                            queryStatus = QueryStatus.NO_DATA;
                        }
                    }
                }
                catch (InvalidOperationException ex)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException ex)
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

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand comm = new SqlCommand("SELECT COUNT(*) FROM dbo.ConsignmentsToBePriced WHERE PricingCompleted IS NULL", connection);
                    int unpricedConsignments = (int)comm.ExecuteScalar();

                    comm = new SqlCommand("SELECT COUNT(*) FROM dbo.ConsignmentsToBeMigrated WHERE PricingCompleted IS NULL", connection);
                    int unmigratedConsignments = (int)comm.ExecuteScalar();

                    var ManifestExportCounts = new ManifestExportCounts(unpricedConsignments, unmigratedConsignments);
                    builder.Add(ManifestExportCounts);
                }
                catch (InvalidOperationException ex)
                {
                    queryStatus = QueryStatus.FAILED_CONNECTION;
                    builder.Clear();
                }
                catch (SqlException ex)
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
        public RetrieveResults<ManifestImportDetailUpdateFailed> ManifestImportDetailUpdate(string Json)
        {
            var builder = ImmutableList.CreateBuilder<ManifestImportDetailUpdateFailed>();
            var queryStatus = QueryStatus.OK;
            var lastHeaderKey = string.Empty;

            JObject con = JObject.Parse(Json);

            JArray items = (JArray)con["ExportedFiles"];
            JObject item;
            JToken jtoken;

            for (int i = 0; i < items.Count; i++)
            {
                item = (JObject)items[i];
                jtoken = item.First;

                string depotNo = string.Empty;
                string exportDateTime = string.Empty;
                while (jtoken != null)

                {
                    switch (((JProperty)jtoken).Name.ToString())
                    {
                        case "DepotNumber":
                            depotNo = ((JProperty)jtoken).Value.ToString();
                            break;
                        case "LastExport":
                            exportDateTime = ((JProperty)jtoken).Value.ToString();
                            break;
                    }

                    jtoken = jtoken.Next;
                }

                // retrieved the depot and export datetime from the Json item record - call the update sproc 
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    try
                    {
                        connection.Open();
                        var cmd = new SqlCommand("dbo.UpdateAutomatedManifestImportDetail", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@depotNumber", depotNo);
                        cmd.Parameters.AddWithValue("@lastExportDate", exportDateTime);

                        int rowsUpdated = cmd.ExecuteNonQuery();

                        if (rowsUpdated <= 0)
                        {
                            var ManifestImportDetailUpdateFailed = new ManifestImportDetailUpdateFailed(depotNo, exportDateTime);
                            builder.Add(ManifestImportDetailUpdateFailed);
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        queryStatus = QueryStatus.FAILED_CONNECTION;
                        builder.Clear();
                    }
                    catch (SqlException ex)
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