using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsVersionMachineMap : ClassMap<EsVersionMachine>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsVersionMachineMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_VERSIONMACHINE");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.Dateaction, "DATEACTION");
            Map(x => x.Version, "VERSION");
            Map(x => x.IsOk, "ISOK");
            Map(x => x.Lastindexcall, "LASTINDEXCALL");

            References(x => x.Machine)
                .Column("MACHINE_ID")
                .Cascade.None();
        }
    }
}
