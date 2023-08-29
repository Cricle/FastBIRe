using DatabaseSchemaReader.DataSchema;
using FastBIRe.Project;
using FastBIRe.Project.Accesstor;
using FastBIRe.Project.Models;
using System.Data.Common;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class FastBIReServiceCollection
    {
#if !NETSTANDARD2_0
        public static IServiceCollection AddJsonDirectoryProjectAccesstor(this IServiceCollection services,
            string path,
            string extensions,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return AddJsonDirectoryProjectAccesstor<string>(services, path, extensions, lifetime);
        }
        public static IServiceCollection AddJsonDirectoryProjectAccesstor<TProject,TId>(this IServiceCollection services,
            string path,
            string extensions,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProject:IProject<TId>
        {
            services.Add(new ServiceDescriptor(typeof(IProjectAccesstor<IProjectAccesstContext<TId>, TProject, TId>),
                (s) => new JsonDirectoryProjectAccesstor<TProject, IProjectAccesstContext<TId>, TId>(path, extensions),
                lifetime));
            return services;
        }
        public static IServiceCollection AddJsonDirectoryProjectAccesstor<TId>(this IServiceCollection services,
            string path,
            string extensions,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return AddJsonDirectoryProjectAccesstor<Project<TId>,TId>(services,path,extensions,lifetime);
        }
#endif
        public static IServiceCollection AddProjectAccesstor<TId>(this IServiceCollection services,
            Func<IServiceProvider, IProjectAccesstor<IProjectAccesstContext<TId>, Project<TId>, TId>> factory,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            services.Add(new ServiceDescriptor(typeof(IProjectAccesstor<IProjectAccesstContext<TId>, Project<TId>, TId>), (s) => factory(s), lifetime));
            return services;
        }
        public static IServiceCollection AddProjectAccesstor<TAccesstor,TProject,TInput, TId>(this IServiceCollection services,
            Func<IServiceProvider, TAccesstor> factory,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
            where TProject:IProject<TId>
            where TAccesstor:IProjectAccesstor<TInput,TProject,TId>
            where TInput : IProjectAccesstContext<TId>
        {
            services.Add(new ServiceDescriptor(typeof(IProjectAccesstor<TInput,TProject,TId>),(s)=> factory(s), lifetime));
            return services;
        }
        public static IServiceCollection AddDataSchema(this IServiceCollection services,
            Func<IProjectAccesstContext<string>, string> databaseName,
            Func<IProjectAccesstContext<string>, string> tableName,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return AddDataSchema<IProjectAccesstContext<string>, string>(services, databaseName, tableName, lifetime);
        }
        public static IServiceCollection AddDataSchema<TId>(this IServiceCollection services,
            Func<IProjectAccesstContext<TId>, string> databaseName,
            Func<IProjectAccesstContext<TId>, string> tableName,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            return AddDataSchema<IProjectAccesstContext<TId>,TId>(services,databaseName, tableName, lifetime);
        }
        public static IServiceCollection AddDataSchema<TInput,TId>(this IServiceCollection services,
            Func<TInput, string> databaseName, 
            Func<TInput, string> tableName,
            ServiceLifetime lifetime= ServiceLifetime.Singleton)
            where TInput :IProjectAccesstContext<TId>
        {
            services.Add(new ServiceDescriptor(typeof(IDataSchema<TInput>),_=>new DelegateDataSchema<TInput>(databaseName,tableName),lifetime));
            return services;
        }
        public static IServiceCollection AddStringToDbConnectionFactory(this IServiceCollection services,
            SqlType sqlType,
            Func<string, DbConnection> dbConnection,
            Func<string, string, DbConnection> dbConnectionWithDatabase,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            services.Add(new ServiceDescriptor(typeof(IStringToDbConnectionFactory), _ => new DelegateStringToDbConnectionFactory(sqlType,dbConnection, dbConnectionWithDatabase), lifetime));
            return services;
        }
    }
}
