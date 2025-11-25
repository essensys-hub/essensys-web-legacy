using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Repository.DTO
{
    /// <summary>
    /// Terminal Essensys
    /// </summary>
    public class EsMachine : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsMachine()
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

        protected string _NoSerie;
        /// <summary>
        /// Numéro de série Essensys
        /// </summary>
        public virtual string NoSerie
        {
            get { return _NoSerie; }
            set { _NoSerie = value; }
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

        protected string _Pkey;
        /// <summary>
        /// Clé privée
        /// </summary>
        public virtual string Pkey
        {
            get { return _Pkey; }
            set { _Pkey = value; }
        }

        protected string _HashedPkey;
        /// <summary>
        /// Clé privée hashée
        /// </summary>
        public virtual string HashedPkey
        {
            get { return _HashedPkey; }
            set { _HashedPkey = value; }
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

        protected DateTime _DateModification;
        /// <summary>
        /// Date de modification
        /// </summary>
        public virtual DateTime DateModification
        {
            get { return _DateModification; }
            set { _DateModification = value; }
        }

        protected bool _IsActive;
        /// <summary>
        /// Est active
        /// </summary>
        public virtual bool IsActive
        {
            get { return _IsActive; }
            set { _IsActive = value; }
        }


        protected bool _AutoriseAlarme;
        /// <summary>
        /// Autorise alarme ou non
        /// </summary>
        public virtual bool AutoriseAlarme
        {
            get { return _AutoriseAlarme; }
            set { _AutoriseAlarme = value; }
        }

        protected IList<EsUser> _Users;
        /// <summary>
        /// Utilisateurs associés
        /// </summary>
        public virtual IList<EsUser> Users
        {
            get { return _Users; }
            set { _Users = value; }
        }
    }
}
