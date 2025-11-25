using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using FluentNHibernate.Cfg;
using System.Configuration;
using FluentNHibernate.Cfg.Db;
using System.Reflection;
using Essensys.Common;

namespace Essensys.Repository
{
    /// <summary>
    /// Gestion de la session Factory NHibernate
    /// </summary>
    public static class EsSessionFactory
    {
        private static ISession _sess;

        public static ISession Sess
        {
            get
            {
                if (_sess == null)
                {
                    _sess = SessionFactory.OpenSession();
                }
                return _sess;
            }
        }

        private static ISessionFactory _SessionFactory;

        public static ISessionFactory SessionFactory
        {
            get
            {
                if (_SessionFactory == null)
                {
                    _SessionFactory = CreateSessionFactory().BuildSessionFactory();
                }
                return _SessionFactory;
            }
        }

        public static void InitSessionFactory()
        {
            _SessionFactory = CreateSessionFactory().BuildSessionFactory();
        }

        /// <summary>
        /// Création de la SessionFactory
        /// </summary>
        /// <returns></returns>
        private static FluentConfiguration CreateSessionFactory()
        {
            // Retrait de la chaine de connexion dans la configuration
            string connectionstring = ConfigurationManager.ConnectionStrings["Essensys"].ConnectionString;
            FluentConfiguration configuration = null;

            if (connectionstring == "" || connectionstring == null)
                return null;

            // Paramétrage NHibernate
            configuration = Fluently.Configure()
                .Database(MsSqlConfiguration.MsSql2005.ConnectionString(connectionstring)
                    .ShowSql()
                    .Cache(c => c.UseQueryCache()
                        .ProviderClass(typeof(NHibernate.Caches.SysCache.SysCacheProvider).AssemblyQualifiedName))
                        )
                .ExposeConfiguration(c =>
                    c.SetProperty("current_session_context_class", "web")
                    .SetProperty("cache.use_second_level_cache", "true")
                    .SetProperty("proxyfactory.factory_class", "NHibernate.ByteCode.LinFu.ProxyFactoryFactory, NHibernate.ByteCode.LinFu"));

            configuration.ExposeConfiguration(c => c.SetListener(NHibernate.Event.ListenerType.Flush, new FlushFixEventListener()));
            configuration.ExposeConfiguration(c => c.SetListener(NHibernate.Event.ListenerType.Autoflush, new AutoFlushFixEventListener()));

            configuration.Mappings(AddMappings);

            return configuration;
        }

        private static void AddMappings(MappingConfiguration mapConfig)
        {
            // Load the assembly where the entities live
            Assembly oEsAssembly = Assembly.GetExecutingAssembly();

            mapConfig.FluentMappings.AddFromAssembly(oEsAssembly);
            
            // Merge the mappings
            mapConfig.MergeMappings();
        }

        public static ISession GetSession()
        {
            try
            {
                if (NHibernate.Context.CurrentSessionContext.HasBind(SessionFactory))
                {
                    return SessionFactory.GetCurrentSession();
                }
                else
                {
                    ISession session = SessionFactory.OpenSession();
                    NHibernate.Context.CurrentSessionContext.Bind(session);
                    return session;
                }
            }
            catch (Exception ex)
            {
                if (_sess == null)
                    _sess = SessionFactory.OpenSession();
                return _sess;
            }
        }
    }
}
