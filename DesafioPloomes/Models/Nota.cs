using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DesafioPloomes.Models
{
    public class Nota
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Resposta { get; set; }
        public string RespostaCorreta { get; set; }
    }
}