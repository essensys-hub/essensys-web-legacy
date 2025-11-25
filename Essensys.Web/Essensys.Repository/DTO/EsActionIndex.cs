using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Repository.DTO
{
    public class EsActionIndex : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsActionIndex()
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

        protected EsAction _Action;
        /// <summary>
        /// Action
        /// </summary>
        public virtual EsAction Action
        {
            get { return _Action; }
            set { _Action = value; }
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
