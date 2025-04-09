using System.Text;

namespace liv_inParis
{
    /// <summary>
    /// Représente le graphe des stations de métro.
    /// </summary>
    public class Graphe
    {
        public List<Lien> Liens { get; }
        public bool AfficherTemps { get; set; } = false;
        public bool ModeColoration { get; set; } = false;
        private Dictionary<Station, int> colorationStations;
        private static readonly Color[] CouleursColoration = new Color[]
        {
            Color.Yellow,
            Color.LightBlue,
            Color.LightGreen,
            Color.Pink,
            Color.Orange,
            Color.Purple, 
            Color.Turquoise,
            Color.LightCoral,
            Color.SandyBrown,
            Color.Lavender
        };

        int rayon = 10;

        public Graphe(List<Lien> connexions)
        {
            Liens = connexions;
        }

        /// <summary>
        /// Implémentation de l'algorithme de Dijkstra pour trouver le chemin le plus court entre deux stations.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public (Dictionary<Station, int> distances, Dictionary<Station, Station> previous, Dictionary<Station, string> ligneArrivee) Dijkstra(Station source)
        {
            var distances = new Dictionary<Station, int>();
            var previous = new Dictionary<Station, Station>();
            var ligneArrivee = new Dictionary<Station, string>(); // Pour stocker la ligne d'arrivée à chaque station
            var queue = new PriorityQueue<Station, int>();

            foreach (var station in Liens.SelectMany(l => new[] { l.Depart, l.Arrivee }).Distinct())
            {
                distances[station] = int.MaxValue;
                previous[station] = null;
                ligneArrivee[station] = null;
                queue.Enqueue(station, int.MaxValue);
            }

            distances[source] = 0;
            ligneArrivee[source] = null;
            queue.Enqueue(source, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var lien in Liens.Where(l => l.Depart == current))
                {
                    var neighbor = lien.Arrivee;
                    int tempsChangement = 0;

                    // Ajouter le temps de changement seulement si on change de ligne
                    if (ligneArrivee[current] != null && lien.Ligne != ligneArrivee[current])
                    {
                        tempsChangement = neighbor.TempsChangement;
                    }

                    int newDist = distances[current] + lien.Temps + tempsChangement;

                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        previous[neighbor] = current;
                        ligneArrivee[neighbor] = lien.Ligne;
                        queue.Enqueue(neighbor, newDist);
                    }
                }
            }

            return (distances, previous, ligneArrivee);
        }

        /// <summary>
        /// Retourne le chemin le plus court entre deux stations et le temps total de trajet. Sert de sous-programme pour Dijkstra.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public (List<Station> path, int totalTime) GetShortestPath(Station source, Station target)
        {
            var distances = new Dictionary<Station, int>();
            var previous = new Dictionary<Station, Station>();
            var ligneArrivee = new Dictionary<Station, string>();
            var queue = new PriorityQueue<Station, int>();

            // Initialisation
            foreach (var station in Liens.SelectMany(l => new[] { l.Depart, l.Arrivee }).Distinct())
            {
                distances[station] = int.MaxValue;
                previous[station] = null;
                ligneArrivee[station] = null;
            }

            distances[source] = 0;
            queue.Enqueue(source, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                foreach (var lien in Liens.Where(l => l.Depart == current))
                {
                    var neighbor = lien.Arrivee;
                    int tempsSupplementaire = 0;

                    // Ajout du temps de changement seulement si on change de ligne
                    if (ligneArrivee[current] != null && lien.Ligne != ligneArrivee[current])
                    {
                        tempsSupplementaire = current.TempsChangement;
                    }

                    int newDist = distances[current] + lien.Temps + tempsSupplementaire;

                    if (newDist < distances[neighbor])
                    {
                        distances[neighbor] = newDist;
                        previous[neighbor] = current;
                        ligneArrivee[neighbor] = lien.Ligne;
                        queue.Enqueue(neighbor, newDist);
                    }
                }
            }

            // Reconstruction du chemin
            var path = new List<Station>();
            var currentPath = target;
            while (currentPath != null)
            {
                path.Insert(0, currentPath);
                currentPath = previous[currentPath];
            }

            return (path, distances[target]);
        }

        /// <summary>
        /// Affiche le chemin sur le graphe.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="path"></param>
        /// <param name="color"></param>
        public void AfficherChemin(Graphics g, List<Station> path, Color color)
        {
            if (path == null || path.Count == 0) return;

            using (var pen = new Pen(color, 10))
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(pen, path[i].Position, path[i + 1].Position);
                }
            }
        }

        /// <summary>
        /// Gere la messageBox qui affiche les liens
        /// </summary>
        public void AfficherLiens()
        {
            StringBuilder sb = new StringBuilder();

            if (Liens == null || Liens.Count == 0)
            {
                sb.AppendLine("Aucun lien n'est présent dans la liste.");
            }
            else
            {
                sb.AppendLine($"Nombre de liens: {Liens.Count}");
                foreach (Lien lien in Liens)
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
        /// Gere l'affichage des liaisons sur le graphe
        /// </summary>
        /// <param name="g"></param>
        /// <param name="stations"></param>
        public void AfficherConnexions(Graphics g, List<Station> stations)
        {
            if (Liens == null || stations == null) return;

            // Dessiner les connexions avec flèches
            foreach (Lien lien in Liens)
            {
                DessinerArc(g, lien); // Appel de la nouvelle méthode
            }
        }

        /// <summary>
        /// Affiche les stations sur le graphe avec leur coloration
        /// </summary>
        public void AfficherStations(Graphics g, List<Station> stations, Station stationSelectionnee)
        {
            // Dessiner les stations et leurs noms
            foreach (var station in stations)
            {
                if (station.Position == null) continue;  // Sécurité si position non définie
                
                Color couleur;
                if (station == stationSelectionnee)
                {
                    couleur = Color.Red; // Toujours en rouge pour la station sélectionnée
                }
                else if (ModeColoration && colorationStations != null && colorationStations.TryGetValue(station, out int indice))
                {
                    // Utilisation de la coloration de graphe
                    couleur = CouleursColoration[indice % CouleursColoration.Length];
                }
                else
                {
                    // Couleur par défaut
                    couleur = Color.White;
                }

                // Dessiner le cercle principal
                using (var brush = new SolidBrush(couleur))
                {
                    g.FillEllipse(brush,
                                 station.Position.X - rayon,
                                 station.Position.Y - rayon,
                                 2 * rayon,
                                 2 * rayon);
                }

                using (var pen = new Pen(Color.Black, 2))
                {
                    g.DrawEllipse(pen,
                                 station.Position.X - rayon,
                                 station.Position.Y - rayon,
                                 2 * rayon,
                                 2 * rayon);

                    // Afficher le nom de la station
                    g.DrawString(station.Nom, new Font("Arial", 10, FontStyle.Regular), Brushes.Black, station.Position.X + rayon + 2, station.Position.Y - rayon);
                }
            }
        }

        private void DessinerArc(Graphics g, Lien lien)
        {
            if (lien?.Depart?.Position == null || lien?.Arrivee?.Position == null)
                return;

            // Calculer les points ajustés pour éviter les chevauchements
            Point p1 = AjusterPoint(lien.Depart.Position, lien.Arrivee.Position);
            Point p2 = AjusterPoint(lien.Arrivee.Position, lien.Depart.Position);

            var couleur = couleursLignes.TryGetValue(lien.Ligne, out Color c) ? c : Color.Gray;

            using (Pen pen = new Pen(couleur, 5))
            {
                // Dessiner la ligne entre les points ajustés
                g.DrawLine(pen, p1, p2);

                // Dessiner la flèche si nécessaire
                if (lien.Unidir)
                {
                    DessinerFleche(g, pen, p1, p2);
                }

                // Afficher le temps de trajet
                if (AfficherTemps)
                {
                    var milieu = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                    g.DrawString($"{lien.Temps} min", new Font("Arial", 8), Brushes.Black, milieu);
                }
            }
        }

        // Ajoutez cette méthode d'ajustement dans la classe Graphe
        private Point AjusterPoint(Point source, Point target)
        {
            double angle = Math.Atan2(target.Y - source.Y, target.X - source.X);
            return new Point(
                (int)(source.X + rayon * Math.Cos(angle)),
                (int)(source.Y + rayon * Math.Sin(angle))
            );
        }

        /// <summary>
        /// Dessine une flèche à l'extrémité d'une ligne pour les arcs unidirectionnels.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="pen"></param>
        /// <param name="pointDepart"></param>
        /// <param name="pointArrivee"></param>
        /// <param name="tailleFleche"></param>
        private void DessinerFleche(Graphics g, Pen pen, Point pointDepart, Point pointArrivee, int tailleFleche = 13)
        {
            // Calcul de l'angle de la ligne
            float angle = (float)Math.Atan2(pointArrivee.Y - pointDepart.Y, pointArrivee.X - pointDepart.X);

            // Points pour la flèche
            PointF[] points = new PointF[3];
            points[0] = pointArrivee;
            points[1] = new PointF(
                pointArrivee.X - tailleFleche * (float)Math.Cos(angle - Math.PI / 6),
                pointArrivee.Y - tailleFleche * (float)Math.Sin(angle - Math.PI / 6));
            points[2] = new PointF(
                pointArrivee.X - tailleFleche * (float)Math.Cos(angle + Math.PI / 6),
                pointArrivee.Y - tailleFleche * (float)Math.Sin(angle + Math.PI / 6));

            // Dessin de la flèche
            g.FillPolygon(pen.Brush, points);
        }

        /// <summary>
        /// Applique l'algorithme de coloration de graphe aux stations
        /// </summary>
        /// <returns>Dictionnaire associant chaque station à une couleur (numéro)</returns>
        public Dictionary<Station, int> ColorerStations()
        {
            var coloration = new Dictionary<Station, int>();
            var stations = Liens.SelectMany(l => new[] { l.Depart, l.Arrivee }).Distinct().ToList();
            
            // Trie les stations par nombre de voisins décroissant (heuristique)
            stations = stations.OrderByDescending(s => 
                Liens.Count(l => l.Depart == s || l.Arrivee == s)).ToList();
            
            foreach (var station in stations)
            {
                // Trouver les couleurs déjà utilisées par les voisins
                var couleursVoisins = new HashSet<int>();
                foreach (var lien in Liens.Where(l => l.Depart == station || l.Arrivee == station))
                {
                    var voisin = lien.Depart == station ? lien.Arrivee : lien.Depart;
                    if (coloration.TryGetValue(voisin, out int couleur))
                        couleursVoisins.Add(couleur);
                }
                
                // Trouver la première couleur disponible
                int couleurStation = 0;
                while (couleursVoisins.Contains(couleurStation))
                    couleurStation++;
                    
                coloration[station] = couleurStation;
            }
            
            colorationStations = coloration;
            return coloration;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public (List<Station> path, int totalTime) GetShortestPathBellmanFord(Station source, Station target)
        {
            var distances = new Dictionary<Station, int>();
            var previous = new Dictionary<Station, Station>();
            var ligneArrivee = new Dictionary<Station, string>(); // Ajout du tracking des lignes comme dans Dijkstra

            // Initialisation
            foreach (var station in Liens.SelectMany(l => new[] { l.Depart, l.Arrivee }).Distinct())
            {
                distances[station] = int.MaxValue;
                previous[station] = null;
                ligneArrivee[station] = null;
            }
            distances[source] = 0;
            
            // Relâchement des arêtes (V-1 itérations)
            for (int i = 0; i < distances.Count - 1; i++)
            {
                foreach (var lien in Liens)
                {
                    if (distances[lien.Depart] != int.MaxValue)
                    {
                        int tempsSupplementaire = 0;
                        
                        // Ajout du temps de changement seulement si on change de ligne
                        if (ligneArrivee[lien.Depart] != null && lien.Ligne != ligneArrivee[lien.Depart])
                        {
                            tempsSupplementaire = lien.Depart.TempsChangement;
                        }
                        
                        int newDist = distances[lien.Depart] + lien.Temps + tempsSupplementaire;
                        
                        if (newDist < distances[lien.Arrivee])
                        {
                            distances[lien.Arrivee] = newDist;
                            previous[lien.Arrivee] = lien.Depart;
                            ligneArrivee[lien.Arrivee] = lien.Ligne;
                        }
                    }
                }
            }

            // Reconstruction du chemin
            var path = new List<Station>();
            var current = target;
            while (current != null)
            {
                path.Insert(0, current);
                current = previous[current];
            }

            return (path, distances[target]); // Pas besoin d'ajouter nbrchangements car temps déjà inclus
        }

        /// <summary>
        /// Dictionnaire des couleurs des lignes de métro.
        /// </summary>
        Dictionary<string, Color> couleursLignes = new Dictionary<string, Color>
            {
                // couleurs Métro
                { "1", Color.FromArgb(255, 223, 0, 36) },      // Jaune (officiellement Pantone 123C)
                { "2", Color.FromArgb(255, 0, 156, 222) },     // Bleu (Pantone 299C)
                { "3", Color.FromArgb(255, 149, 196, 56) },    // Vert (Pantone 376C)
                { "3bis", Color.FromArgb(255, 0, 180, 148) },    // Turquoise (Pantone 3278C)
                { "4", Color.FromArgb(255, 189, 29, 157) },    // Violet (Pantone 253C)
                { "5", Color.FromArgb(255, 237, 125, 46) },    // Orange (Pantone 158C)
                { "6", Color.FromArgb(255, 0, 166, 152) },     // Vert clair (Pantone 3265C)
                { "7", Color.FromArgb(255, 214, 0, 123) },     // Rose (Pantone 212C)
                { "7bis", Color.FromArgb(255, 124, 190, 58) },   // Vert clair (Pantone 360C)
                { "8", Color.FromArgb(255, 196, 96, 161) },    // Lilas (Pantone 223C)
                { "9", Color.FromArgb(255, 206, 220, 0) },     // Moutarde (Pantone 381C)
                { "10", Color.FromArgb(255, 223, 176, 20) },   // Jaune sable (Pantone 7405C)
                { "11", Color.FromArgb(255, 137, 50, 123) },   // Marron (Pantone 506C)
                { "12", Color.FromArgb(255, 0, 147, 68) },     // Vert émeraude (Pantone 348C)
                { "13", Color.FromArgb(255, 0, 176, 217) },    // Bleu clair (Pantone 311C)
                { "14", Color.FromArgb(255, 99, 52, 142) }
            };
    }
}