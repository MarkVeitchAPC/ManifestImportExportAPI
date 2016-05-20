using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Web.Http;

namespace ManifestImportExportAPI.Controllers
{
    public class ImportController
    {
        [RoutePrefix("api/ImportManifest")]
        [Authorize(Roles = "MIE")]
        public class ImportManifestController : BaseApiController
        {
            public IImportManifestRepository Repository { get; set; }

            public ImportManifestController()
            { }

            public ImportManifestController(IImportManifestRepository repo)
            {
                Repository = repo;
            }

            // POST: api/ImportManifest
            [HttpPost]
            //18-05-2016 public IHttpActionResult Post([FromBody]string Json, Int32 depotNumber, bool scottishManifest)
            public IHttpActionResult Post([FromBody]JObject Json)
            {
                RetrieveResults<ManifestImportDetailUpdateFailed> FailedImportManifestResult = new RetrieveResults<ManifestImportDetailUpdateFailed>();

                //18-05-2016 FailedImportManifestResult = Repository.ImportManifest(Json, depotNumber, scottishManifest);
                FailedImportManifestResult = Repository.ImportManifest(Json);

                if (FailedImportManifestResult.Status != QueryStatus.OK) return BadRequest();  //NotFound();

                return Ok(ReturnedData<ManifestImportDetailUpdateFailed>.ReturnData(FailedImportManifestResult.Results));
            }
        }
    }
}