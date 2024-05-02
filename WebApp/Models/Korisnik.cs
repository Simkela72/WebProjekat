using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApp.Models
{
    public enum Pol
    {
        Muski=1,
        Zenski=2,
        Drugo=3
    }
    public enum Uloga
    {
        Posetilac=1,
        Trener=2,
        Vlasnik=3
    }

    public class Korisnik
    {

        public string KorisnickoIme { get; set; }
        public string Lozinka { get; set; }
        public string Ime { get; set; }
        public string Prezime { get; set; }
        public Pol Pol { get; set; }
        public string Email { get; set; }
        public DateTime? DatumRodjenja { get; set; }
        public Uloga Uloga { get; set; }





    }
}