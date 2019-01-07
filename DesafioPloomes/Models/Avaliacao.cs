using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DesafioPloomes.Models
{
    public class Avaliacao
    {
        public int IdAvaliacao { get; set; }
        public string NomeAvaliacao { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Fim { get; set; }
        public int IdVaga { get; set; }
    }
}