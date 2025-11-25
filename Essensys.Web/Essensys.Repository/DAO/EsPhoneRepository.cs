using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using Essensys.Repository.DTO;

namespace Essensys.Repository.DAO
{
    public class EsPhoneRepository : BaseRepository<EsPhone>
    {
        public EsPhoneRepository(ISession session)
            : base(session)
        {
        }
    }
}
