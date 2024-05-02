using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public class PosetilacTrening
    {
        public int PosetilacTreningID { get; set; }
        public int? GrupniTreningID { get; set; }
        public string Posetilac { get; set; }
    }
}