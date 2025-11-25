using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsClemachineMap : ClassMap<EsClemachine>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsClemachineMap()
        {
            Cache.ReadWrite().Region("longterm");
            Table("ES_CLEMACHINE");
            Id(x => x.Id, "ID")
            .GeneratedBy.Identity();
            Map(x => x.Cle, "CLE");
            Map(x => x.Dateactivation, "DATEACTIVATION");
            Map(x => x.Dategeneration, "DATEGENERATION");

            References(x => x.Machine)
                .Column("MACHINE_ID")
                .Cascade.None();
        }
    }
}
