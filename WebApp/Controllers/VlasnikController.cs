using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApp.Models;
using MySql.Data.MySqlClient;
namespace WebApp.Controllers
{
    public class VlasnikController : Controller
    {
        private string connectionString = "Server=localhost;Port=3306;Database=sys;Uid=root;Pwd=spajk2002;";
        [HttpGet]
        public ActionResult Index()
        {
            string vlasnikUsername = Session["Username"] as string;
            List<string> lista = GetFitnesCentarVlasnik(vlasnikUsername);
            ViewBag.FitnessCenters = lista;
            List<FitnesCentar> fitnesCentars = GetFitnesCentars(vlasnikUsername);
            List<string> zaposleni = GetZaposleni(vlasnikUsername);
            ViewBag.Zaposleni = zaposleni;
            ViewBag.FitnessCenters1 = fitnesCentars;
            ViewBag.Vlasnik = vlasnikUsername;
            return View();
        }
        [HttpPost]
        public ActionResult AddTrainer(string KorIm, string Loz, string Im, string Prez, string Ul, string Email, DateTime Dat, string fitnessCenterName)
        {
            // Validate the model
            if (!ModelState.IsValid)
            {
                // If model validation fails, redirect back to the Index action
                return RedirectToAction("Index");
            }

            Pol pol;
            if (Ul =="Muski")
            {
                pol = Pol.Muski;
            }
            else if (Ul == "Zenski")
            {
                pol = Pol.Zenski;
            }else if (Ul =="Drugo")
            {
                pol = Pol.Drugo;
            }
            else
            {
                pol = Pol.Drugo;
            }
            
            
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Insert trainer details into the korisnik table
                string insertTrainerQuery = "INSERT INTO korisnik (KorisnickoIme, Lozinka, Ime, Prezime, Pol, Email, DatumRodjenja, Uloga) " +
                                            "VALUES (@KorisnickoIme, @Lozinka, @Ime, @Prezime, @Pol, @Email, @DatumRodjenja, @Uloga)";
                using (var insertTrainerCommand = new MySqlCommand(insertTrainerQuery, connection))
                {
                    insertTrainerCommand.Parameters.AddWithValue("@KorisnickoIme", KorIm);
                    insertTrainerCommand.Parameters.AddWithValue("@Lozinka", Loz);
                    insertTrainerCommand.Parameters.AddWithValue("@Ime", Im);
                    insertTrainerCommand.Parameters.AddWithValue("@Prezime", Prez);
                    insertTrainerCommand.Parameters.AddWithValue("@Pol", pol); 
                    insertTrainerCommand.Parameters.AddWithValue("@Email", Email);
                    insertTrainerCommand.Parameters.AddWithValue("@DatumRodjenja", Dat);
                    insertTrainerCommand.Parameters.AddWithValue("@Uloga", Uloga.Trener);

                    insertTrainerCommand.ExecuteNonQuery();
                }

                // Insert association between trainer and fitness center into the trener_centar table
                string insertAssociationQuery = "INSERT INTO trener_centar (Fitnes_Centar, Trener) " +
                                                 "VALUES (@FitnesCentar, @Trener)";
                using (var insertAssociationCommand = new MySqlCommand(insertAssociationQuery, connection))
                {
                    insertAssociationCommand.Parameters.AddWithValue("@FitnesCentar", fitnessCenterName);
                    insertAssociationCommand.Parameters.AddWithValue("@Trener", KorIm);

                    insertAssociationCommand.ExecuteNonQuery();
                }
            }

            // Redirect to the Index action after adding the trainer
            return RedirectToAction("Index");
        }

        
        private List<string> GetFitnesCentarVlasnik(string vlasnikUsername)
        {
            List<string> lista = new List<string>();
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM fitnes_centar WHERE Vlasnik=@vlasnik";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@vlasnik", vlasnikUsername);
                using(MySqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string naziv = reader["Naziv"].ToString();
                        lista.Add(naziv);
                    }
                }
            }
            return lista;
        }
        public ActionResult ViewFitnesCentar(string Naziv)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM fitnes_centar WHERE Naziv = @naziv"; // Ensure parameter name matches
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@naziv", Naziv); // Ensure parameter name matches
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
                        return View(fitnes_centar);
                    }
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult DeleteFitnesCentar(string Naziv)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string deleteUsersQuery = "DELETE FROM korisnik WHERE KorisnickoIme IN (SELECT Trener FROM trener_centar WHERE Fitnes_Centar = @naziv)";
                using (MySqlCommand cmd = new MySqlCommand(deleteUsersQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@naziv", Naziv);
                    cmd.ExecuteNonQuery();
                }
                
               
                string deleteFitnesCentarQuery = "DELETE FROM fitnes_centar WHERE Naziv = @naziv";
                using (MySqlCommand cmd = new MySqlCommand(deleteFitnesCentarQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@naziv", Naziv);
                    cmd.ExecuteNonQuery();
                }

                // Step 3: Delete users associated with the fitness center from korisnik table
                
            }

            return RedirectToAction("Index");
        }



        [HttpPost]
        public ActionResult UpdateFitnesCenter(FitnesCentar model)
        {
            string query = @"
        UPDATE fitnes_centar 
        SET 
            Adresa = @adresa,
            Cena_mesec = @cenaMesec,
            Cena_godina = @cenaGodina,
            Jedan_trening = @jedanTrening,
            Grupni_trening = @grupniTrening,
            Cena_personal_trener = @cenaPersonalTrener
        WHERE Naziv = @naziv;
    ";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                using (MySqlCommand cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@adresa", model.Adresa);
                    cmd.Parameters.AddWithValue("@cenaMesec", model.CenaMesec);
                    cmd.Parameters.AddWithValue("@cenaGodina", model.CenaGodina);
                    cmd.Parameters.AddWithValue("@jedanTrening", model.JedanTrening);
                    cmd.Parameters.AddWithValue("@grupniTrening", model.GrupniTrening);
                    cmd.Parameters.AddWithValue("@cenaPersonalTrener", model.CenaPersonalTrener);
                    cmd.Parameters.AddWithValue("@naziv", model.Naziv);

                    int rowsAffected = cmd.ExecuteNonQuery();
                }
            }

            return RedirectToAction("Index");
        }
        
        public ActionResult AddFitnesCenterView()
        {
            string vlasnikUsername = Session["Username"] as string;
            ViewBag.Vlasnik = vlasnikUsername;
            return View();
        }

        [HttpPost]

        public ActionResult AddFitnesCentar(string Naziv, string Adresa, int GodinaOtvaranja, decimal CenaMesec, decimal CenaGodina, decimal JedanTrening, decimal GrupniTrening, decimal CenaPersonalTrener, string Vlasnik)
        {
            using(MySqlConnection connection=new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "INSERT INTO fitnes_centar (Naziv, Adresa, GodinaOtvaranja, Cena_mesec, Cena_godina, Jedan_trening, Grupni_trening, Cena_personal_trener, Vlasnik) VALUES (@Naziv, @Adresa, @GodinaOtvaranja, @CenaMesec, @CenaGodina, @JedanTrening, @GrupniTrening, @CenaPersonalTrener, @Vlasnik)";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Naziv", Naziv);
                cmd.Parameters.AddWithValue("@Adresa", Adresa);
                cmd.Parameters.AddWithValue("@GodinaOtvaranja", GodinaOtvaranja);
                cmd.Parameters.AddWithValue("@CenaMesec", CenaMesec);
                cmd.Parameters.AddWithValue("@CenaGodina", CenaGodina);
                cmd.Parameters.AddWithValue("@JedanTrening", JedanTrening);
                cmd.Parameters.AddWithValue("@GrupniTrening", GrupniTrening);
                cmd.Parameters.AddWithValue("@CenaPersonalTrener", CenaPersonalTrener);
                cmd.Parameters.AddWithValue("@Vlasnik", Vlasnik);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }

        private List<FitnesCentar> GetFitnesCentars(string vlasnikUsername)
        {
            List<FitnesCentar> fitnesCentri = new List<FitnesCentar>();
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT * FROM fitnes_centar WHERE Vlasnik=@vlasnik";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@vlasnik", vlasnikUsername);
                using (MySqlDataReader reader = cmd.ExecuteReader())
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

        public List<string> GetZaposleni(string Vlasnik)
        {
            List<string> zaposleni = new List<string>();

            string queryString = @"
            SELECT tc.Trener
            FROM trener_centar tc
            JOIN fitnes_centar fc ON tc.Fitnes_Centar = fc.Naziv
            JOIN korisnik k ON fc.Vlasnik = k.KorisnickoIme
            WHERE k.Uloga = 'Vlasnik' AND k.KorisnickoIme = @OwnerUsername;
        ";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            using (MySqlCommand command = new MySqlCommand(queryString, connection))
            {
                command.Parameters.AddWithValue("@OwnerUsername", Vlasnik);

                connection.Open();

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        zaposleni.Add(reader["Trener"].ToString());
                    }
                }
            }

            return zaposleni;
        }

        public ActionResult BlockEmployee(string KorisnickoIme)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                string query = "DELETE FROM korisnik WHERE KorisnickoIme=@Ime";
                MySqlCommand cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@Ime", KorisnickoIme);
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
}
