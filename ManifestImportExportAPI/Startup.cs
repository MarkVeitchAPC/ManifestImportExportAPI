using ManifestImportExportAPI.Infrastructure;
using ManifestImportExportAPI.Provider;
using Microsoft.Owin.Cors;
using Newtonsoft.Json.Serialization;
using NLog;
using Owin;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;
using Unity.WebApi;




namespace ManifestImportExportAPI
{
    public class Startup
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration httpConfig = new HttpConfiguration();
            httpConfig.DependencyResolver = new UnityDependencyResolver(UnityConfig.GetConfiguredContainer());

            ConfigureOAuthTokenGeneration(app);
            ConfigureOAuthTokenConsumption(app);
            ConfigureWebApi(httpConfig);

            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(httpConfig);

        }

        private void ConfigureOAuthTokenGeneration(IAppBuilder app)
        {
            logger.Log(LogLevel.Debug, "ConfigureOAuthTokenGeneration");
            // OAuth 2.0 Bearer Access Token Generation
            app.CreatePerOwinContext<APCUserManager>(APCUserManager.Create);
            app.CreatePerOwinContext<APCRoleManager>(APCRoleManager.Create);
            var oauthServerOptions = new APCOAuthServerOptions();
            logger.Log(LogLevel.Debug, "AllowInsecureHttp: {0}", oauthServerOptions.AllowInsecureHttp);
            logger.Log(LogLevel.Debug, "TokenEndpointPath: {0}", oauthServerOptions.TokenEndpointPath);
            app.UseOAuthAuthorizationServer(oauthServerOptions);
        }

        private void ConfigureOAuthTokenConsumption(IAppBuilder app)
        {
            logger.Log(LogLevel.Debug, "ConfigureOAuthTokenConsumptions");
            // Api controllers with an [Authorize] attribute will be validated with JWT
            var jwtOptions = new APCJwtOptions();
            app.UseJwtBearerAuthentication(jwtOptions);

        }

        private void ConfigureWebApi(HttpConfiguration config)
        {

            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                //TODO - is this correct?
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            //Filter Model Validation Errors
            config.Filters.Add(new ValidateModelStateAttributeFilter());

            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}