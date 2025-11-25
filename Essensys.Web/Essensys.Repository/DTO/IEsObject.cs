using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Essensys.Repository.DTO
{
    /// <summary>
    /// Objet générique Essensys
    /// </summary>
    public interface IEsObject
    {
        /// <summary>
        /// Identifiant de l'objet
        /// </summary>
        int Id { get; set; }
    }
}
