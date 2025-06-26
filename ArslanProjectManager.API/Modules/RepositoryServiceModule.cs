using ArslanProjectManager.Core.Repositories;
using ArslanProjectManager.Core.Services;
using ArslanProjectManager.Core.UnitOfWork;
using ArslanProjectManager.Repository;
using ArslanProjectManager.Repository.Repositories;
using ArslanProjectManager.Repository.UnitOfWork;
using ArslanProjectManager.Service.Mappings;
using ArslanProjectManager.Service.Services;
using Autofac;
using System.Reflection;
using Module = Autofac.Module;

namespace ArslanProjectManager.API.Modules
{
    public class RepositoryServiceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(GenericRepository<>))
                .As(typeof(IGenericRepository<>))
                .InstancePerLifetimeScope();
            builder.RegisterGeneric(typeof(GenericService<>))
                .As(typeof(IGenericService<>))
                .InstancePerLifetimeScope();
            builder.RegisterType<UnitOfWork>()
                .As<IUnitOfWork>();
            builder.RegisterType<TokenHandler>()
                .As<ITokenHandler>();

            // Register all services and repositories that end with "Service" or "Repository"
            var apiAssembly = Assembly.GetExecutingAssembly();
            var repositoryAssembly = Assembly.GetAssembly(typeof(ProjectManagerDbContext));
            var serviceAssembly = Assembly.GetAssembly(typeof(MapProfile));

            builder.RegisterAssemblyTypes(apiAssembly, repositoryAssembly!, serviceAssembly!)
                .Where(t => t.Name.EndsWith("Service") || t.Name.EndsWith("Repository") || t.Name.Contains("TokenHandler"))
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();
        }
    }
}