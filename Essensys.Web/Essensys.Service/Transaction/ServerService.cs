using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DAO;
using Essensys.Repository;
using Essensys.Repository.DTO;
using NHibernate;
using NHibernate.Criterion;
using Essensys.Service.Response;
using System.IO;
using System.Configuration;
using System.Web;

namespace Essensys.Service.Transaction
{
    /// <summary>
    /// Service de paramétrage du serveur
    /// </summary>
    public class ServerService
    {
        protected EsDataIndexRepository _rep;
        protected EsVersionRepository _repv;

        public ServerService()
        {
            _rep = new EsDataIndexRepository(EsSessionFactory.GetSession());
            _repv = new EsVersionRepository(EsSessionFactory.GetSession());
        }

        /// <summary>
        /// Retourne le numéro de version coté serveur
        /// </summary>
        /// <returns></returns>
        public EsVersion GetVersionServer()
        {
            ICriteria c = EsSessionFactory.GetSession().CreateCriteria<EsVersion>();
            c.AddOrder(Order.Desc("Id"));
            c.SetMaxResults(1);
            return _repv.FindByCriteria(c);
        }

        public EsVersionPart GetVersionPart(int id)
        {
            EsVersion v = GetVersionServer();
            if (v == null)
                return null;
            using (StreamReader fs = File.OpenText(ConfigurationManager.AppSettings["Essensys.VersionDirectory"] + "\\" + v.Filename))
            {
                string c = fs.ReadToEnd();
                int buffer = Convert.ToInt32(ConfigurationManager.AppSettings["Essensys.BufferLength"]);
                string part = "";
                int next = 0;
                if (buffer + id > c.Length)
                {
                    part = c.Substring(id, c.Length - id);
                    next = 0;
                }
                else{
                    part = c.Substring(id, buffer);
                    next = id + buffer;
                }
                EsVersionPart vp = new EsVersionPart();
                vp.index = id;
                vp.nextindex = next;
                vp.content = part;

                // Log
                EsMachine m = HttpContext.Current.Session["Machine"] as EsMachine;
                new VersionMachineService().LogGetVersion(m, "V" + v.Id, id);
                return vp;
            }
        }

        /// <summary>
        /// Retourne la liste des index référencés
        /// </summary>
        /// <returns>Liste de clés</returns>
        public List<int> GetDataIndex()
        {
            return _rep.List(di => di.IsActive, 0, 0)
                .Select(di => Convert.ToInt32(di.IndexKey))
                .ToList();
        }

        /// <summary>
        /// Retourne un index enregistré au niveau du serveur
        /// </summary>
        /// <param name="key">Clé</param>
        /// <returns>Objet index</returns>
        public EsDataIndex FindDataIndex(int key)
        {
            return _rep.FindBy(di => di.IsActive && di.IndexKey == key.ToString());
        }
    }
}
