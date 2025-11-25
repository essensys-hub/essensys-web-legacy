using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    public class EsStateIndex : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsStateIndex()
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

        protected string _Value;
        /// <summary>
        /// Identifiant
        /// </summary>
        public virtual string Value
        {
            get { return _Value; }
            set { _Value = value; }
        }

        protected EsState _State;
        /// <summary>
        /// Etat
        /// </summary>
        [ScriptIgnore]
        public virtual EsState State
        {
            get { return _State; }
            set { _State = value; }
        }

        protected EsDataIndex _Index;
        /// <summary>
        /// Index
        /// </summary>
        public virtual EsDataIndex Index
        {
            get { return _Index; }
            set { _Index = value; }
        }
    }
}
