using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using MySql.Data.MySqlClient;

namespace WebApp.Controllers
{
    public class AuthController : Controller
    {
        private string connectionString = "Server=localhost;Port=3306;Database=sys;Uid=root;Pwd=spajk2002;";

       

        public ActionResult Login()
        {
            List<FitnesCentar> fitnes_centri = GetFitnesCentars();
            ViewBag.FitnesCentri = fitnes_centri;
            return View();
        }

        [HttpPost]
        public ActionResult Login(Korisnik model)
        {
            if (ModelState.IsValid)
            {
                // Authenticate user
                var role = AuthenticateUser(model.KorisnickoIme, model.Lozinka);
                if (role != null)
                {   
                    Session["Username"] = model.KorisnickoIme;
                    Session["UserRole"] = role.ToString();

                    switch (role)
                    {
                        case Uloga.Posetilac:
                            return RedirectToAction("Index", "Posetilac");
                        case Uloga.Trener:
                            return RedirectToAction("Index", "Trener");
                        case Uloga.Vlasnik:
                            return RedirectToAction("Index", "Vlasnik");
                        default:
                            ModelState.AddModelError("", "Invalid role.");
                            break;
                    }

                    

                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password.");
                }
            }
            return View(model);
        }

        public ActionResult Details(string name)
        {
            // Retrieve fitness center details based on the provided name
            var fitnesCentar = GetFitnesCentar(name);
            if (fitnesCentar == null)
            {
                return HttpNotFound(); // Or some other appropriate action
            }

            return View(fitnesCentar);
        }

        private Uloga? AuthenticateUser(string username, string password)
        {
            Uloga? role = null;
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT Uloga FROM korisnik WHERE KorisnickoIme = @username AND Lozinka = @password";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@username", username);
                cmd.Parameters.AddWithValue("@password", password);
                var result = cmd.ExecuteScalar();
                if (result != null)
                {
                    role = (Uloga)Enum.Parse(typeof(Uloga), result.ToString());
                }
            }
            return role;
        }

        private List<FitnesCentar> GetFitnesCentars()
        {
            List<FitnesCentar> fitnesCentri = new List<FitnesCentar>();
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM fitnes_centar";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                using(MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FitnesCentar fitnes_centar = new FitnesCentar
                        {
                            Naziv = reader["Naziv"].ToString(),
                            Adresa = reader["Adresa"].ToString(),
                            GodinaOtvaranja = Convert.ToDecimal(reader["GodinaOtvaranja"]),
                            CenaMesec = Convert.ToDecimal(reader["Cena_mesec"]),
                            CenaGodina = Convert.ToDecimal(reader["Cena_godina"]),
                            JedanTrening = Convert.ToDecimal(reader["Jedan_trening"]),
                            GrupniTrening = Convert.ToDecimal(reader["Grupni_trening"]),
                            CenaPersonalTrener = Convert.ToDecimal(reader["Cena_personal_trener"]),
                            Vlasnik = reader["Vlasnik"].ToString()

                        };
                        fitnesCentri.Add(fitnes_centar);
                    }
                }
            }
            return fitnesCentri;
        }

        private FitnesCentar GetFitnesCentar(string name)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM fitnes_centar WHERE Naziv = @naziv"; // Ensure parameter name matches
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@naziv", name); // Ensure parameter name matches
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        FitnesCentar fitnes_centar = new FitnesCentar
                        {
                            Naziv = reader["Naziv"].ToString(),
                            Adresa = reader["Adresa"].ToString(),
                            GodinaOtvaranja = Convert.ToDecimal(reader["GodinaOtvaranja"]),
                            CenaMesec = Convert.ToDecimal(reader["Cena_mesec"]),
                            CenaGodina = Convert.ToDecimal(reader["Cena_godina"]),
                            JedanTrening = Convert.ToDecimal(reader["Jedan_trening"]),
                            GrupniTrening = Convert.ToDecimal(reader["Grupni_trening"]),
                            CenaPersonalTrener = Convert.ToDecimal(reader["Cena_personal_trener"]),
                            Vlasnik = reader["Vlasnik"].ToString()
                        };
                        return fitnes_centar;
                    }
                }
            }
            return null;

        }

        public ActionResult RegisterView()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string KorisnickoIme, string Lozinka, string Ime, string Prezime, string Pol, string Email, DateTime DatumRodjenja)
        {
            string uloga = "Posetilac";

            string query = "INSERT INTO korisnik (KorisnickoIme, Lozinka, Ime, Prezime, Pol, Email, DatumRodjenja, Uloga) " +
                           "VALUES (@KorisnickoIme, @Lozinka, @Ime, @Prezime, @Pol, @Email, @DatumRodjenja, @Uloga)";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();


                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@KorisnickoIme", KorisnickoIme);
                    cmd.Parameters.AddWithValue("@Lozinka", Lozinka);
                    cmd.Parameters.AddWithValue("@Ime", Ime);
                    cmd.Parameters.AddWithValue("@Prezime", Prezime);
                    cmd.Parameters.AddWithValue("@Pol", Pol);
                    cmd.Parameters.AddWithValue("@Email", Email);
                    cmd.Parameters.AddWithValue("@DatumRodjenja", DatumRodjenja);
                    cmd.Parameters.AddWithValue("@Uloga", uloga);

                    cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Login");
        }


    }
}