using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    public class EsState : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsState()
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


        protected string _Version;
        /// <summary>
        /// Version du boitier
        /// </summary>
        public virtual string Version
        {
            get { return _Version; }
            set { _Version = value; }
        }

        protected bool _Completed;
        /// <summary>
        /// Enregistrement des index effectué
        /// </summary>
        public virtual bool Completed
        {
            get { return _Completed; }
            set { _Completed = value; }
        }

        protected DateTime _StateDate;
        /// <summary>
        /// Date de l'état
        /// </summary>
        public virtual DateTime StateDate
        {
            get { return _StateDate; }
            set { _StateDate = value; }
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
