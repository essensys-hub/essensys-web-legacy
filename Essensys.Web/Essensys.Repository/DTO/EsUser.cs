using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Essensys.Repository.DTO
{
    /// <summary>
    /// Compte utilisateur
    /// </summary>
    public class EsUser : IEsObject
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsUser()
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

        protected string _Mail;
        /// <summary>
        /// EMail
        /// </summary>
        [Required(ErrorMessage="Veuillez entrer votre adresse email.")]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Veuillez entrer une adresse email valide.")]
        [Remote("CheckEMail", "Account", ErrorMessage="Cet email est déjà associé à un compte Essensys.")]
        public virtual string Mail
        {
            get { return _Mail; }
            set { _Mail = value; }
        }


        protected string _ReMail;
        /// <summary>
        /// EMail
        /// </summary>
        [Required(ErrorMessage = "Veuillez confirmer votre adresse email.")]
        [Compare("Mail", ErrorMessage="L'email doit correspondre.")]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Veuillez entrer une adresse email valide.")]
        public virtual string ReMail
        {
            get { return _ReMail; }
            set { _ReMail = value; }
        }

        protected string _Mail2;
        /// <summary>
        /// EMail modifié
        /// </summary>
        [Required(ErrorMessage="Veuillez entrer votre adresse email.")]
        [RegularExpression("^[a-z0-9_\\+-]+(\\.[a-z0-9_\\+-]+)*@[a-z0-9-]+(\\.[a-z0-9]+)*\\.([a-z]{2,4})$", ErrorMessage = "Veuillez entrer une adresse email valide.")]
        public virtual string Mail2
        {
            get { return _Mail; }
            set { _Mail = value; }
        }

        protected string _Password;
        /// <summary>
        /// Mot de passe
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer un mot de passe.")]
        public virtual string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }


        protected string _Password2;
        /// <summary>
        /// Mot de passe
        /// </summary>
        public virtual string Password2
        {
            get { return _Password2; }
            set { _Password2 = value; }
        }

        protected string _NewPassword;
        /// <summary>
        /// Nouveau Mot de passe
        /// </summary>
        public virtual string NewPassword
        {
            get { return _NewPassword; }
            set { _NewPassword = value; }
        }

        protected string _ConfirmPassword;
        /// <summary>
        /// Confirmation de Mot de passe
        /// </summary>
        [Required(ErrorMessage = "Veuillez saisir à nouveau le mot de passe.")]
        [Compare("Password", ErrorMessage = "Veuillez saisir le même mot de passe.")]
        public virtual string ConfirmPassword
        {
            get { return _ConfirmPassword; }
            set { _ConfirmPassword = value; }
        }

        protected string _ConfirmNewPassword;
        /// <summary>
        /// Confirmation de Mot de passe
        /// </summary>
        [Compare("NewPassword", ErrorMessage = "Veuillez saisir le même mot de passe.")]
        public virtual string ConfirmNewPassword
        {
            get { return _ConfirmNewPassword; }
            set { _ConfirmNewPassword = value; }
        }

        protected bool _rememberMe;
        /// <summary>
        /// Activation Cookie
        /// </summary>
        public virtual bool RememberMe
        {
            get { return _rememberMe; }
            set { _rememberMe = value; }
        }

        protected string _NoSerie;
        /// <summary>
        /// Numéro de série Essensys
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer la clef d'activation de votre Essensys")]
        [Remote("CheckNoSerie", "Account", ErrorMessage = "Cette clef d'activation est déjà associée à un compte Essensys ou bien elle est invalide.")]
        public virtual string NoSerie
        {
            get { return _NoSerie; }
            set { _NoSerie = value; }
        }

        protected string _Nom;
        /// <summary>
        /// Nom
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer votre nom.")]
        [StringLength(255, ErrorMessage="Le nom ne doit pas excéder 255 caractères.")]
        public virtual string Nom
        {
            get { return _Nom; }
            set { _Nom = value; }
        }

        protected string _Prenom;
        /// <summary>
        /// Prenom
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer votre prénom.")]
        [StringLength(255, ErrorMessage = "Le prénom ne doit pas excéder 255 caractères.")]
        public virtual string Prenom
        {
            get { return _Prenom; }
            set { _Prenom = value; }
        }

        protected string _Adr1;
        /// <summary>
        /// Adresse 1
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer votre adresse.")]
        [StringLength(255, ErrorMessage = "L'adresse ne doit pas excéder 255 caractères.")]
        public virtual string Adr1
        {
            get { return _Adr1; }
            set { _Adr1 = value; }
        }

        protected string _Adr2;
        /// <summary>
        /// Adresse 2
        /// </summary>
        [StringLength(255, ErrorMessage = "L'adresse ne doit pas excéder 255 caractères")]
        public virtual string Adr2
        {
            get { return _Adr2; }
            set { _Adr2 = value; }
        }
        protected string _Cp;
        /// <summary>
        /// Code postal
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer votre code postal.")]
        [StringLength(5, ErrorMessage = "Le code postal ne doit pas excéder 5 caractères.")]
        [RegularExpression("^(F-)?((2[A|B])|[0-9]{2})[0-9]{3}$", ErrorMessage="Le code postal doit être conforme.")]
        public virtual string Cp
        {
            get { return _Cp; }
            set { _Cp = value; }
        }
        protected string _Ville;
        /// <summary>
        /// Ville
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrer votre ville.")]
        [StringLength(255, ErrorMessage = "La ville ne doit pas excéder 255 caractères")]
        public virtual string Ville
        {
            get { return _Ville; }
            set { _Ville = value; }
        }
        protected string _Phone;
        /// <summary>
        /// Téléphone
        /// </summary>
        [RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Veuillez saisir un numéro de téléphone")]
        public virtual string Phone
        {
            get { return _Phone; }
            set { _Phone = value; }
        }
        protected string _Question;
        /// <summary>
        /// Question
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrez votre question de sécurité.")]
        [StringLength(255, ErrorMessage = "La question de sécurité ne doit pas excéder 255 caractères.")]
        public virtual string Question
        {
            get { return _Question; }
            set { _Question = value; }
        }
        protected string _Reponse;
        /// <summary>
        /// Réponse
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrez la réponse à la question de sécurité.")]
        [StringLength(255, ErrorMessage = "La réponse à la question de sécurité ne doit pas excéder 255 caractères.")]
        public virtual string Reponse
        {
            get { return _Reponse; }
            set { _Reponse = value; }
        }

        protected string _Reponse2;
        /// <summary>
        /// Réponse
        /// </summary>
        [Required(ErrorMessage = "Veuillez entrez de nouveau la réponse à la question de sécurité.")]
        [Compare("Reponse", ErrorMessage = "La réponse à la question de sécurité doit être identique.")]
        [StringLength(255, ErrorMessage = "La réponse à la question de sécurité ne doit pas excéder 255 caractères.")]
        public virtual string Reponse2
        {
            get { return _Reponse2; }
            set { _Reponse2 = value; }
        }

        protected bool _SendInfos;
        /// <summary>
        /// Autorisation d'envoi d'Informations
        /// </summary>
        public virtual bool SendInfos
        {
            get { return _SendInfos; }
            set { _SendInfos = value; }
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

        protected bool _Isvalid;
        /// <summary>
        /// Compte validé
        /// </summary>
        public virtual bool Isvalid
        {
            get { return _Isvalid; }
            set { _Isvalid = value; }
        }

        protected bool _Obsolete;
        /// <summary>
        /// Compte clôturé
        /// </summary>
        public virtual bool Obsolete
        {
            get { return _Obsolete; }
            set { _Obsolete = value; }
        }

        protected DateTime _Datecreation;
        /// <summary>
        /// Date de création du compte
        /// </summary>
        public virtual DateTime Datecreation
        {
            get { return _Datecreation; }
            set { _Datecreation = value; }
        }

        protected DateTime _Datecloture;
        /// <summary>
        /// Date de clôture du compte
        /// </summary>
        public virtual DateTime Datecloture
        {
            get { return _Datecloture; }
            set { _Datecloture = value; }
        }

        protected DateTime _Lastaccess;
        /// <summary>
        /// Date du dernier accès
        /// </summary>
        public virtual DateTime Lastaccess
        {
            get { return _Lastaccess; }
            set { _Lastaccess = value; }
        }

        protected string _Guid;
        /// <summary>
        /// Guid
        /// </summary>
        public virtual string Guid
        {
            get { return _Guid; }
            set { _Guid = value; }
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
    }
}
