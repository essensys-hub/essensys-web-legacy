using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FluentNHibernate.Mapping;

namespace Essensys.Repository.DTO
{
    public class EsMachineMap : ClassMap<EsMachine>
    {
        /// <summary>
        /// Constructeur par défaut
        /// </summary>
        public EsMachineMap()
        {
            Cache.ReadWrite().Region("longterm");
                Table("ES_MACHINE");
                Id(x => x.Id, "ID")
                .GeneratedBy.Identity();
            Map(x => x.Pkey, "PKEY");
            Map(x => x.HashedPkey, "HASHEDPKEY");
            Map(x => x.NoSerie, "NOSERIE");
            Map(x => x.Version, "VERSION");
            Map(x => x.AutoriseAlarme, "AUTORISEALARME");
            
            Map(x => x.DateCreation, "DATECREATION");
            Map(x => x.DateModification, "DATEMODIFICATION");
            Map(x => x.IsActive, "ISACTIVE");
            HasMany<EsUser>(x => x.Users)
                .KeyColumn("MACHINE_ID")
                .Inverse()
                .Cascade.SaveUpdate()
                .AsBag();
        }
    }
}
