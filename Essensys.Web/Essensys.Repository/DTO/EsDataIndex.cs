using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace Essensys.Repository.DTO
{
    /// <summary>
    /// Index de données à échanger
    /// </summary>
    public class EsDataIndex : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsDataIndex()
        {
        }

        protected int _Id;
        /// <summary>
        /// Identifiant
        /// </summary>
        [ScriptIgnore]
        public virtual int Id
        {
            get { return _Id; }
            set { _Id = value; }
        }

        protected string _IndexKey;
        /// <summary>
        /// Clé
        /// </summary>
        public virtual string IndexKey
        {
            get { return _IndexKey; }
            set { _IndexKey = value; }
        }

        protected bool _IsActive;

        /// <summary>
        /// Est actif
        /// </summary>
        [ScriptIgnore]
        public virtual bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }

        protected DateTime _DateCreation;
        /// <summary>
        /// Date de création
        /// </summary>
        [ScriptIgnore]
        public virtual DateTime DateCreation
        {
            get { return _DateCreation; }
            set { _DateCreation = value; }
        }

        protected DateTime _DateModification;
        /// <summary>
        /// Date de modification
        /// </summary>
        [ScriptIgnore]
        public virtual DateTime DateModification
        {
            get { return _DateModification; }
            set { _DateModification = value; }
        }
    }
}
