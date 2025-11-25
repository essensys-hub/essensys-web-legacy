using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsUserMap : ClassMap<EsUser>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsUserMap()
        {
            Cache.ReadWrite().Region("longterm");
            Table("ES_USER");
            Id(x => x.Id, "ID")
            .GeneratedBy.Identity();
            Map(x => x.Mail, "MAIL");
            Map(x => x.Password, "PASSWORD");
            Map(x => x.Nom, "NOM");
            Map(x => x.Prenom, "PRENOM");
            Map(x => x.Adr1, "ADR1");
            Map(x => x.Adr2, "ADR2");
            Map(x => x.Cp, "CP");
            Map(x => x.Ville, "VILLE");
            Map(x => x.Phone, "PHONE");
            Map(x => x.Question, "QUESTION");
            Map(x => x.Reponse, "REPONSE");
            Map(x => x.Isvalid, "ISVALID");
            Map(x => x.SendInfos, "SENDINFOS");
            Map(x => x.Obsolete, "OBSOLETE");
            Map(x => x.Datecreation, "DATECREATION");
            Map(x => x.Datecloture, "DATECLOTURE");
            Map(x => x.Lastaccess, "LASTACCESS");
            Map(x => x.Guid, "GUID");

            References(x => x.Machine)
                .Column("MACHINE_ID")
                .Cascade.None();
        }
    }
}
