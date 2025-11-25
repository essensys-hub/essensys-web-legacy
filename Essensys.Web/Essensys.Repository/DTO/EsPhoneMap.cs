using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsPhoneMap : ClassMap<EsPhone>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsPhoneMap()
        {
            Cache.ReadWrite().Region("longterm");
            Table("ES_PHONE");
            Id(x => x.Id, "ID")
            .GeneratedBy.Identity();
            Map(x => x.Phone, "PHONE");
            Map(x => x.Datecreation, "DATECREATION");
            Map(x => x.Nom, "NOM");
            Map(x => x.Datemodification, "DATEMODIFICATION");
            Map(x => x.Sendmail, "SENDMAIL");
            Map(x => x.AlerteAlarmeSent, "ALERTEALARMESENT");
            Map(x => x.AlerteLvSent, "ALERTELVSENT");
            Map(x => x.AlerteLlSent, "ALERTELLSENT");
            Map(x => x.AlerteNoSync, "ALERTENOSYNC");

            References(x => x.UsrId)
                .Column("USR_ID")
                .Cascade.None();
        }
    }
}
