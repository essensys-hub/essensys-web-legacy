using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    public class EsVersionMachine : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsVersionMachine()
        {
        }

        protected int _Id;
        /// <summary>
        /// Identifiant
        /// </summary>
        public virtual int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        protected DateTime _Dateaction;
        /// <summary>
        /// Date de dernière action
        /// </summary>
        public virtual DateTime Dateaction
        {
            get { return _Dateaction; }
            set { _Dateaction = value; }
        }

        protected string _Version;
        /// <summary>
        /// Version concernée
        /// </summary>
        public virtual string Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

        protected bool _IsOk;
        /// <summary>
        /// Téléchargement terminé
        /// </summary>
        public virtual bool IsOk
        {
            get { return _IsOk; }
            set { _IsOk = value; }
        }

        protected int _Lastindexcall;
        /// <summary>
        /// Dernier index téléchargé
        /// </summary>
        public virtual int Lastindexcall
        {
            get { return _Lastindexcall; }
            set { _Lastindexcall = value; }
        }

        protected EsMachine _Machine;
        /// <summary>
        /// Machine
        /// </summary>
        [ScriptIgnore]
        public virtual EsMachine Machine
        {
            get { return _Machine; }
            set { _Machine = value; }
        }
    }
}
