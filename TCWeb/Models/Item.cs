using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TCWeb.Models {
    /// <summary>
    /// Clase modelo para un candidato.
    /// </summary>
    public class Item {
        [Required]
        public string Nombre { get; set; }
        public string URL { get; set; }
        public string Puesto { get; set; }
        public string Numero { get; set; }
        public string Correo { get; set; }
    }
}
