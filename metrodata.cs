using System.Globalization;
using System.Text;

namespace liv_inParis
{
    public class MetroDataService
    {
        private const int PorteMaillotX = 325;
        private const int PorteMaillotY = 350;
        private const double EchelleLatitude = 8750;
        private const double EchelleLongitude = 11000;

        public List<Station> Stations { get; } = new();
        public List<Lien> Connexions { get; } = new();
        string cheminArcs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "arcs.csv");
        string cheminStations = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "stations.csv");
        public Dictionary<int, Station> dico { get; } = new();

        public MetroDataService()
        {
            dico = dicostations();
            //AfficherDictionnaire(dico);

            Stations = LireStations();
            CalculerPositions(Stations);
            Connexions = LireLiens();
        }

        /// <summary>
        /// Lit les données des stations à partir d'un fichier CSV et les stocke dans un dictionnaire.
        /// </summary>
        /// <returns>Un dictionnaire contenant les stations avec leur ID comme clé.</returns>
        public Dictionary<int, Station> dicostations()
        {
            var stations = new Dictionary<int, Station>();
            using var reader = new StreamReader(cheminStations, Encoding.UTF8);
            reader.ReadLine(); // Ignorer l'en-tête


            while (!reader.EndOfStream)
            {
                var ligne = reader.ReadLine();
                var valeurs = ligne.Split(';');

                if (!string.IsNullOrWhiteSpace(valeurs[0]) && int.TryParse(valeurs[0], out int id))
                {
                    string nom = valeurs[1];
                    float latitude = float.Parse(valeurs[2], CultureInfo.InvariantCulture);
                    float longitude = float.Parse(valeurs[3], CultureInfo.InvariantCulture);

                    Station station = new Station(id, nom, latitude, longitude);
                    stations[id] = station;

                }
            }
            return stations;
        }

        /// <summary>
        /// Lit les données des stations à partir d'un fichier CSV et les stocke dans une liste.
        /// </summary>
        /// <returns>Une liste contenant toutes les stations.</returns>
        public List<Station> LireStations()
        {
            var stations = new List<Station>();
            using var reader = new StreamReader(cheminStations, Encoding.UTF8);
            reader.ReadLine();

            while (!reader.EndOfStream)
            {
                var ligne = reader.ReadLine();
                var valeurs = ligne.Split(';');

                if (!string.IsNullOrWhiteSpace(valeurs[0]) && int.TryParse(valeurs[0], out int id))
                {
                    string nom = valeurs[1];
                    float latitude = float.Parse(valeurs[2], CultureInfo.InvariantCulture);
                    float longitude = float.Parse(valeurs[3], CultureInfo.InvariantCulture);

                    Station station = new Station(id, nom, latitude, longitude);
                    stations.Add(station);

                }
            }
            return stations;
        }

        /// <summary>
        /// Lit les données des liens entre les stations à partir d'un fichier CSV et les stocke dans une liste.
        /// </summary>
        /// <returns>Une liste contenant tous les liens entre les stations.</returns>
        public List<Lien> LireLiens()
        {
            Random rand = new();
            using var reader = new StreamReader(cheminArcs, Encoding.UTF8);
            reader.ReadLine(); // Ignorer l'en-tête

            while (!reader.EndOfStream)
            {
                var ligne = reader.ReadLine();
                var valeurs = ligne.Split(';');

                if (string.IsNullOrWhiteSpace(valeurs[0]) || string.IsNullOrWhiteSpace(valeurs[3]) || string.IsNullOrWhiteSpace(valeurs[4]))
                {
                    // Gérer l'erreur ou ignorer la ligne
                    continue;
                }

                int departId = int.Parse(valeurs[0]);
                int arriveeId = int.Parse(valeurs[3]);

                Station depart = Stations.FirstOrDefault(s => s.Id == departId);
                Station arrivee = Stations.FirstOrDefault(s => s.Id == arriveeId);

                if (depart != null && arrivee != null)
                {
                    int temps = rand.Next(1, 5);
                    bool unidir = int.Parse(valeurs[4].Trim()) == 1;
                    string ligneM = valeurs[5].Trim();

                    Connexions.Add(new Lien(depart, arrivee, temps, unidir, ligneM));

                    // Si le lien n'est PAS unidirectionnel, on ajoute le lien inverse
                    if (!unidir)
                    {
                        Connexions.Add(new Lien(arrivee, depart, temps, false, ligneM));
                    }
                }
                else
                {
                    // Gérer l'erreur ou ignorer la ligne
                    continue;
                }
            }

            return Connexions;
        }

        /// <summary>
        /// Calcule les positions des stations par rapport à la station Porte Maillot.
        /// </summary>
        /// <param name="stations">La liste des stations dont les positions doivent être calculées.</param>
        private void CalculerPositions(List<Station> stations)
        {
            foreach (var station in stations)
            {
                if (station.Nom == "porteMaillot") continue;

                // Calcul de la position par rapport à Porte Maillot
                double deltaLon = station.Longitude - stations[0].Longitude;
                double deltaLat = station.Latitude - stations[0].Latitude;

                // Applique l'échelle
                int x = PorteMaillotX - (int)(deltaLon * EchelleLongitude);
                int y = PorteMaillotY + (int)(deltaLat * EchelleLatitude);

                station.Position = new Point(y, x); //axes inversés
            }
        }

        /// <summary>
        /// Affiche la liste des stations dans une boîte de dialogue.
        /// </summary>
        public void AfficherStations()
        {
            StringBuilder sb = new StringBuilder();

            if (Stations == null || Stations.Count == 0)
            {
                sb.AppendLine("Aucun station n'est présent dans la liste.");
            }
            else
            {
                sb.AppendLine($"Nombre de liens: {Stations.Count}");
                foreach (Station station in Stations)
                {
                    if (station != null && station.Nom != null && station.Position != null)
                    {
                        sb.AppendLine($"Id: {station.Id} ({station.Nom}), Position: {station.Position} )");
                    }
                    else
                    {
                        sb.AppendLine("Station invalide (null ou avec position nulles)");
                    }
                }
            }

            MessageBox.Show(sb.ToString(), "Liste des Stations");
        }

        /// <summary>
        /// Affiche la liste des liens entre les stations dans une boîte de dialogue.
        /// </summary>
        public void AfficherLiens()
        {
            StringBuilder sb = new StringBuilder();

            if (Connexions == null || Connexions.Count == 0)
            {
                sb.AppendLine("Aucun lien n'est présent dans la liste.");
            }
            else
            {
                sb.AppendLine($"Nombre de liens: {Connexions.Count}");
                foreach (Lien lien in Connexions)
                {
                    if (lien != null && lien.Depart != null && lien.Arrivee != null)
                    {
                        sb.AppendLine($"Départ: {lien.Depart.Nom} ({lien.Depart.Position}), Arrivée: {lien.Arrivee.Nom} ({lien.Arrivee.Position})");
                    }
                    else
                    {
                        sb.AppendLine("Lien invalide (null ou avec stations nulles)");
                    }
                }
            }

            MessageBox.Show(sb.ToString(), "Liste des Liens");
        }

        /// <summary>
        /// Affiche les informations détaillées d'une station dans une boîte de dialogue.
        /// </summary>
        /// <param name="station">La station dont les informations doivent être affichées.</param>
        public void AfficherInfosStation(Station station)
        {
            if (station == null) return;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"=== STATION {station.Nom} ===\n");
            sb.AppendLine($"ID: {station.Id}");
            sb.AppendLine($"Position: {station.Position}");
            sb.AppendLine($"Coordonnées: {station.Latitude:F6}, {station.Longitude:F6}");

            // Stations desservies (adjacentes)
            sb.AppendLine("\n--- STATIONS DESSERVIES ---");
            foreach (var lien in Connexions.Where(l => l.Depart == station || l.Arrivee == station))
            {
                var autreStation = lien.Depart == station ? lien.Arrivee : lien.Depart;
                sb.AppendLine($"- {autreStation.Nom}");
            }

            MessageBox.Show(sb.ToString(), $"Informations sur {station.Nom}");
        }
    }
}