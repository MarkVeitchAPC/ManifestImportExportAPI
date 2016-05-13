using System;
using ManifestImportExportAPI.Models;
using System.Collections.Immutable;

//*********************************************************************************
//**
//** IT IS LIKELY THAT THIS PART OF THE WEBSERVICE WILL NOT BE PUT INTO ACTION 
//** THE CURRENT THINKING IS THAT THE RETREIVE LATEST MANIFEST LIST WILL BE
//** DONE ON THE CLIENT SIDE ON WWW2.  THIS REMAINS HERE FOR POSSIBLE FUTURE USE 
//**
//********************************************************************************* 

namespace ManifestImportExportAPI.Repositories
{
    public class ManifestRepository : IManifestRepository
    {
        private string _connectionString;

        public ManifestRepository()
        {
            _connectionString = System.Configuration.ConfigurationManager.AppSettings["connectionString"];
        }

        public RetrieveResults<ManifestList> RetrieveManifestList(DateTime date)
        {
            var builder = ImmutableList.CreateBuilder<ManifestList>();
            var queryStatus = QueryStatus.OK;
            var lastHeaderKey = string.Empty;
            ManifestList manifest = new ManifestList();

            //using (SqlConnection connection = new SqlConnection(_connectionString))
            //{
            //    try
            //    {
            //        connection.Open();

            //        var cmd = new SqlCommand("dbo.RetrieveInPostCons", connection);
            //        cmd.CommandType = CommandType.StoredProcedure;
            //        cmd.Parameters.AddWithValue("@sendDate", date);

            //        var dr = cmd.ExecuteReader();
            //        if (dr.HasRows)
            //        {
            //            while (dr.Read())
            //            {
            //                var headerKey = Parse.ParseString(dr["HeaderKey"].ToString());
            //                if (headerKey != lastHeaderKey)
            //                {
            //                    manifest = new ManifestList(ParseConsignor(dr), ParseConsignmentHeader(dr), ParseConsignee(dr));
            //                    builder.Add(manifest);
            //                    lastHeaderKey = headerKey;
            //                }
            //                else
            //                {
            //                    var inPostConsignmentItem = ParseConsignmentItem(dr);
            //                    manifest.Consignment.InPostConsignmentItem.Add(inPostConsignmentItem);
            //                }
            //            }
            //        }
            //        else
            //        {
            //            queryStatus = QueryStatus.NO_DATA;
            //        }
            //    }
            //    catch (InvalidOperationException ex)
            //    {

            //        queryStatus = QueryStatus.FAILED_CONNECTION;
            //        builder.Clear();
            //    }
            //    catch (SqlException ex)
            //    {

            //        queryStatus = QueryStatus.FAIL;
            //        builder.Clear();
            //    }
            //}

            return new RetrieveResults<ManifestList>(builder.ToImmutableList(), queryStatus);
        }
    }
}