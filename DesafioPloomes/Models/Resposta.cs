using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DesafioPloomes.Models
{
    public class Resposta
    {
        public int IdResposta { get; set; }
        public string NomeResposta { get; set; }
        public int IdQuestao { get; set; }
        public int IdUser { get; set; }
    }
}