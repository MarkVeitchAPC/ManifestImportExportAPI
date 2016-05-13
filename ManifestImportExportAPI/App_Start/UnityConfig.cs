using ManifestImportExportAPI.Repositories;
using ManifestImportExportAPI.Repositories.RepositoryInterfaces;
using Microsoft.Practices.Unity;
using System;
using System.Web.Http;
using Unity.WebApi;

namespace ManifestImportExportAPI
{
    public static class UnityConfig
    {
        #region Unity Container
        private static Lazy<IUnityContainer> container = new Lazy<IUnityContainer>(() =>
        {
            var container = new UnityContainer();
            RegisterTypes(container);
            return container;
        });

        /// <summary>
        /// Gets the configured Unity container.
        /// </summary>
        public static IUnityContainer GetConfiguredContainer()
        {
            return container.Value;
        }
        #endregion

        public static void RegisterTypes(IUnityContainer container)
        {
            var apcAuthRepoManager = new ContainerControlledLifetimeManager();
            container.RegisterType<IAPCAuthRepository, APCAuthRepository>("UserRepo", apcAuthRepoManager);

        }
    }
}