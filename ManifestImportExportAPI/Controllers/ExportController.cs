using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using ManifestImportExportAPI.Repositories.RepositoryInterfaces;
using System;
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
                var exportManifestConsResult = Repository.ExportManifest(depotNumber, manifestDate);

                if (exportManifestConsResult.Status == QueryStatus.NO_DATA) return NotFound();

                return Ok(ReturnedData<ManifestExport>.ReturnData(exportManifestConsResult.Results));
            }

            // GET: api/ExportManifest
            [HttpGet]
            [Route("Barcodes")]
            public IHttpActionResult GetBarcodes(Int32 depotNumber, DateTime manifestDate)
            {
                var exportManifestBarcodeResult = Repository.ExportManifestBarcode(depotNumber, manifestDate);

                if (exportManifestBarcodeResult.Status == QueryStatus.NO_DATA) return NotFound();

                return Ok(ReturnedData<ManifestExportBarcode>.ReturnData(exportManifestBarcodeResult.Results));
            }

            // GET: api/ExportManifest
            [HttpGet]
            [Route("Counts")]
            public IHttpActionResult GetCounts()
            {
                var exportManifestCountsResult = Repository.ManifestExportCounts();

                if (exportManifestCountsResult.Status == QueryStatus.NO_DATA) return NotFound();

                return Ok(ReturnedData<ManifestExportCounts>.ReturnData(exportManifestCountsResult.Results));
            }


            // Updates a database called [Continuus].[dbo].[AutomatedManifestImportDetail] which holds import and export summaries
            //
            // PUT: api/ExportManifest
            [HttpPut]
            [Route("LastExportTimestamp")]
            public IHttpActionResult Put([FromBody]string json)
            {
                var failedImportDetailUpdateResult = Repository.ManifestImportDetailUpdate(json);

                if (failedImportDetailUpdateResult.Status != QueryStatus.OK) return BadRequest();  //NotFound();

                return Ok(ReturnedData<ManifestImportDetailUpdateFailed>.ReturnData(failedImportDetailUpdateResult.Results));
            }
        }
    }
}