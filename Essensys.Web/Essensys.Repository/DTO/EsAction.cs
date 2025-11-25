using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Repository.DTO
{
    public class EsAction : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsAction()
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

        protected DateTime _DateCreation;
        /// <summary>
        /// Date de création
        /// </summary>
        public virtual DateTime DateCreation
        {
            get { return _DateCreation; }
            set { _DateCreation = value; }
        }

        protected string _Guid;
        /// <summary>
        /// GUID Action
        /// </summary>
        public virtual string Guid
        {
            get { return _Guid; }
            set { _Guid = value; }
        }

        protected bool _IsDone;
        /// <summary>
        /// GUID Action
        /// </summary>
        public virtual bool IsDone
        {
            get { return _IsDone; }
            set { _IsDone = value; }
        }

        protected string _ActionType;
        /// <summary>
        /// Type d'action
        /// </summary>
        public virtual string ActionType
        {
            get { return _ActionType; }
            set { _ActionType = value; }
        }

        protected string _ActionInfo;
        /// <summary>
        /// Info Action
        /// </summary>
        public virtual string ActionInfo
        {
            get { return _ActionInfo; }
            set { _ActionInfo = value; }
        }

        protected EsMachine _Machine;
        /// <summary>
        /// Machine associée
        /// </summary>
        public virtual EsMachine Machine
        {
            get { return _Machine; }
            set { _Machine = value; }
        }

        protected IList<EsActionIndex> _Indexes;
        /// <summary>
        /// Index associés
        /// </summary>
        public virtual IList<EsActionIndex> Indexes
        {
            get { return _Indexes; }
            set { _Indexes = value; }
        }
    }
}
