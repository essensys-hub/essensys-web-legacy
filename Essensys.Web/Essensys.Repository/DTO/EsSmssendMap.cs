using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsSmssendMap : ClassMap<EsSmssend>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsSmssendMap()
        {
            Cache.ReadWrite().Region("longterm");
            Table("ES_SMSSEND");
            Id(x => x.Id, "ID")
            .GeneratedBy.Identity();
            Map(x => x.Phone, "PHONE");
            Map(x => x.Message, "MESSAGE");
            Map(x => x.Datesent, "DATESENT");

            References(x => x.UsrId)
                .Column("USR_ID")
                .Cascade.None();
        }
    }
}
