using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using ManifestImportExportAPI.Repositories.RepositoryInterfaces;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace ManifestImportExportAPI.Controllers
{
    public class ExportController
    {
        [RoutePrefix("api/ExportManifest")]
        [Authorize(Roles = "MIE")]
        public class ExportManifestController : BaseApiController
        {
            public IExportManifestRepository Repository { get; set; }

            public ExportManifestController()
            { }

            public ExportManifestController(IExportManifestRepository repo)
            {
                Repository = repo;
            }

            // GET: api/ExportManifest
            [HttpGet]
            [Route("Consignments")]
            public IHttpActionResult Get(Int32 depotNumber, DateTime manifestDate)
            {
                RetrieveResults<ManifestExport> ExportManifestConsResult = new RetrieveResults<ManifestExport>();

                ExportManifestConsResult = Repository.ExportManifest(depotNumber, manifestDate);

                if (ExportManifestConsResult.Status == QueryStatus.NO_DATA) return NotFound();

                return Ok(ReturnedData<ManifestExport>.ReturnData(ExportManifestConsResult.Results));
            }

            // GET: api/ExportManifest
            [HttpGet]
            [Route("Barcodes")]
            public IHttpActionResult GetBarcodes(Int32 depotNumber, DateTime manifestDate)
            {
                RetrieveResults<ManifestExportBarcode> ExportManifestBarcodeResult = new RetrieveResults<ManifestExportBarcode>();

                ExportManifestBarcodeResult = Repository.ExportManifestBarcode(depotNumber, manifestDate);

                if (ExportManifestBarcodeResult.Status == QueryStatus.NO_DATA) return NotFound();

                return Ok(ReturnedData<ManifestExportBarcode>.ReturnData(ExportManifestBarcodeResult.Results));
            }

            // GET: api/ExportManifest
            [HttpGet]
            [Route("Counts")]
            public IHttpActionResult GetCounts()
            {
                RetrieveResults<ManifestExportCounts> ExportManifestCountsResult = new RetrieveResults<ManifestExportCounts>();

                ExportManifestCountsResult = Repository.ManifestExportCounts();

                if (ExportManifestCountsResult.Status == QueryStatus.NO_DATA) return NotFound();

                return Ok(ReturnedData<ManifestExportCounts>.ReturnData(ExportManifestCountsResult.Results));
            }


            // Updates a database called [Continuus].[dbo].[AutomatedManifestImportDetail] which holds import and export summaries
            //
            // PUT: api/ExportManifest
            [HttpPut]
            [Route("LastExportTimestamp")]
            public IHttpActionResult Put([FromBody]string Json)
            {
                RetrieveResults<ManifestImportDetailUpdateFailed> FailedImportDetailUpdateResult = new RetrieveResults<ManifestImportDetailUpdateFailed>();

                FailedImportDetailUpdateResult = Repository.ManifestImportDetailUpdate(Json);

                if (FailedImportDetailUpdateResult.Status != QueryStatus.OK) return BadRequest();  //NotFound();

                return Ok(ReturnedData<ManifestImportDetailUpdateFailed>.ReturnData(FailedImportDetailUpdateResult.Results));
            }
        }
    }
}