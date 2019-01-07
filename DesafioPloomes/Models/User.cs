using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DesafioPloomes.Models
{
    public class User
    {
        public int IdUser { get; set; }
        public string Nome { get; set; }
        public string Hash { get; set; }
        public int IdCargo { get; set; }
    }
}