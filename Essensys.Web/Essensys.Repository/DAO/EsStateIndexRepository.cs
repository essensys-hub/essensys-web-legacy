using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Essensys.Repository.DTO;
using NHibernate;

namespace Essensys.Repository.DAO
{
    public class EsStateIndexRepository : BaseRepository<EsStateIndex>
    {
        public EsStateIndexRepository(ISession session)
            : base(session)
        {
        }
    }
}
