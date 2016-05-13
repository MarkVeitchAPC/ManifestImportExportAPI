using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace ManifestImportExportAPI.Controllers
{
    public class ImportController
    {
        [RoutePrefix("api/ImportManifest")]
        public class ImportManifestController : BaseApiController
        {
            public IImportManifestRepository Repository { get; set; }

            public ImportManifestController(IImportManifestRepository repo)
            {
                Repository = repo;
            }

            // POST: api/ImportManifest
            [HttpPost]
            public IHttpActionResult Post([FromBody]string Json, Int32 depotNumber, bool scottishManifest)
            {
                RetrieveResults<ManifestImportDetailUpdateFailed> FailedImportManifestResult = new RetrieveResults<ManifestImportDetailUpdateFailed>();

                FailedImportManifestResult = Repository.ImportManifest(Json, depotNumber, scottishManifest);

                if (FailedImportManifestResult.Status != QueryStatus.OK) return BadRequest();  //NotFound();

                return Ok(ReturnedData<ManifestImportDetailUpdateFailed>.ReturnData(FailedImportManifestResult.Results));
            }
        }
    }
}