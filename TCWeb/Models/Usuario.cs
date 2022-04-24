using System;
using System.ComponentModel.DataAnnotations;

namespace TCWeb.Models {
    public class Usuario {
        public int id { get; set; }

        [Required]
        public string usuario { get; set; }

        [Required]
        public string contrasena { get; set; }

        public int intentos { get; set; }

        public decimal nivelSeg { get; set; }

        public DateTime? fechaReg { get; set; }
    }

    public class Credenciales {
        public string username { get; set; }
        public string password { get; set; }
    }
}
