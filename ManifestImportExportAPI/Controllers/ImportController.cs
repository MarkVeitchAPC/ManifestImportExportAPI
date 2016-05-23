using ManifestImportExportAPI.Models;
using ManifestImportExportAPI.Repositories;
using Newtonsoft.Json.Linq;
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

            [HttpPost]
            public IHttpActionResult Post([FromBody]JObject json)
            {
                var failedImportManifestResult = Repository.ImportManifest(json);

                if (failedImportManifestResult.Status != QueryStatus.OK) return BadRequest();  //NotFound();

                return Ok(ReturnedData<ManifestImportDetailUpdateFailed>.ReturnData(failedImportManifestResult.Results));
            }
        }
    }
}