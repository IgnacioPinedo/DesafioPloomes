using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DesafioPloomes.Models
{
    public class Questao
    {
        public int IdQuestao { get; set; }
        public string NomeQuestao { get; set;}
        public string RespostaCorreta { get; set; }
        public int IdAvaliacao { get; set; }
    }
}