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
        /// Affiche un chemin sur le graphe avec une couleur spécifiée et un label explicatif
        /// </summary>
        /// <param name="g">Le contexte graphique</param>
        /// <param name="chemin">Le chemin à afficher</param>
        /// <param name="couleur">La couleur du tracé</param>
        /// <param name="label">Le texte descriptif à afficher</param>
        public void AfficherCheminAvecLabel(Graphics g, List<Station> chemin, Color couleur, string label)
        {
            if (chemin == null || chemin.Count < 2) return;

            using (var pen = new Pen(couleur, 3))
            {
                // Créer un léger décalage aléatoire pour éviter que les chemins ne se superposent exactement
                Random rnd = new Random(couleur.GetHashCode()); // Utilise la couleur comme seed pour avoir un décalage constant
                int decalageX = rnd.Next(-5, 5);
                int decalageY = rnd.Next(-5, 5);

                // Tracer le chemin
                for (int i = 0; i < chemin.Count - 1; i++)
                {
                    if (chemin[i].Position != null && chemin[i + 1].Position != null)
                    {
                        Point p1 = new Point(chemin[i].Position.X + decalageX, chemin[i].Position.Y + decalageY);
                        Point p2 = new Point(chemin[i + 1].Position.X + decalageX, chemin[i + 1].Position.Y + decalageY);

                        g.DrawLine(pen, p1, p2);

                        // Dessiner une flèche pour montrer le sens du trajet
                        if (i == chemin.Count - 2) // Uniquement sur le dernier segment
                        {
                            double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
                            DessinerFleche(g, pen, p1, p2, 13);
                        }
                    }
                }
            }

            // Dessiner le label explicatif
            using (Font font = new Font("Arial", 10, FontStyle.Bold))
            using (SolidBrush brush = new SolidBrush(couleur))
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(200, Color.White))) // Fond semi-transparent
            {
                // Position du label (en haut à gauche du chemin + décalage selon la couleur)
                Station premiere = chemin.First();
                int posX = premiere.Position.X + 10;
                int posY = premiere.Position.Y - 30;

                // Tracer un fond blanc semi-transparent pour améliorer la lisibilité
                SizeF textSize = g.MeasureString(label, font);
                g.FillRectangle(bgBrush, posX - 2, posY - 2, textSize.Width + 4, textSize.Height + 4);

                // Dessiner le texte
                g.DrawString(label, font, brush, posX, posY);
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
        public void AfficherStations(Graphics g, List<Station> stations, Station stationSelectionnee = null, bool afficherNoms = true)
        {
            if (stations == null) return;

            using (var brush = new SolidBrush(Color.White))
            using (var pen = new Pen(Color.Black, 2))
            {
                foreach (var station in stations)
                {
                    if (station.Position == null) continue;

                    // Utiliser les couleurs de coloration si le mode est activé
                    if (ModeColoration && colorationStations != null && colorationStations.ContainsKey(station))
                    {
                        int colorIndex = colorationStations[station] % CouleursColoration.Length;
                        brush.Color = CouleursColoration[colorIndex];
                    }
                    else
                    {
                        brush.Color = Color.White;
                    }

                    // Dessiner la station
                    int x = station.Position.X;
                    int y = station.Position.Y;
                    g.FillEllipse(brush, x - rayon, y - rayon, 2 * rayon, 2 * rayon);
                    
                    // Si la station est sélectionnée, utiliser une couleur différente pour le contour
                    if (station == stationSelectionnee)
                    {
                        pen.Color = Color.Red;
                        pen.Width = 3;
                    }
                    else
                    {
                        pen.Color = Color.Black;
                        pen.Width = 2;
                    }
                    
                    g.DrawEllipse(pen, x - rayon, y - rayon, 2 * rayon, 2 * rayon);
                    
                    // Ne pas dessiner les noms si afficherNoms est false
                    // Cette partie est maintenant conditionnelle
                    if (afficherNoms)
                    {
                        using (var font = new Font("Arial", 8))
                        using (var textBrush = new SolidBrush(Color.Black))
                        {
                            var format = new StringFormat
                            {
                                Alignment = StringAlignment.Center,
                                LineAlignment = StringAlignment.Center
                            };
                            g.DrawString(station.Nom, font, textBrush, x, y + rayon + 10, format);
                        }
                    }
                }
            }
        }

        // Ajouter une nouvelle méthode pour afficher uniquement les noms des stations
        public void AfficherNomsStations(Graphics g, List<Station> stations, Point panOffset, float zoomFactor)
        {
            if (stations == null) return;

            using (var font = new Font("Arial", 8))
            using (var textBrush = new SolidBrush(Color.Black))
            {
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                foreach (var station in stations)
                {
                    if (station.Position == null) continue;

                    // Calculer la position du nom en tenant compte du décalage et du zoom
                    int x = (int)(station.Position.X * zoomFactor + panOffset.X);
                    int y = (int)(station.Position.Y * zoomFactor + panOffset.Y);
                    
                    // Position légèrement décalée vers le bas par rapport à la station
                    y += (int)(rayon * zoomFactor) + 10;

                    // Afficher le nom de la station à l'échelle normale
                    g.DrawString(station.Nom, font, textBrush, x, y, format);
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
        /// Implémente l'algorithme de Floyd-Warshall pour trouver les plus courts chemins entre toutes les paires de stations.
        /// </summary>
        /// <returns>Un tuple contenant la matrice des distances et la matrice des prédécesseurs</returns>// À ajouter dans votre classe Graphe
        public (Dictionary<(Station, Station), int> distances, Dictionary<(Station, Station), Station> next, Dictionary<(Station, Station), string> ligneArrivee) FloydWarshallAvecChangements()
{
    var stations = Liens.SelectMany(l => new[] { l.Depart, l.Arrivee }).Distinct().ToList();
    var distances = new Dictionary<(Station, Station), int>();
    var next = new Dictionary<(Station, Station), Station>();
    var ligneArrivee = new Dictionary<(Station, Station), string>();

    // Initialisation
    foreach (var s1 in stations)
    {
        foreach (var s2 in stations)
        {
            distances[(s1, s2)] = s1 == s2 ? 0 : int.MaxValue;
            next[(s1, s2)] = null;
            ligneArrivee[(s1, s2)] = null;
        }
    }

    // Ajouter les distances pour les connexions existantes
    foreach (var lien in Liens)
    {
        var tempsTotal = lien.Temps;
        distances[(lien.Depart, lien.Arrivee)] = tempsTotal;
        next[(lien.Depart, lien.Arrivee)] = lien.Arrivee;
        ligneArrivee[(lien.Depart, lien.Arrivee)] = lien.Ligne;
    }

    // Algorithme de Floyd-Warshall avec prise en compte des changements de ligne
    foreach (var k in stations)
    {
        foreach (var i in stations)
        {
            foreach (var j in stations)
            {
                if (distances[(i, k)] != int.MaxValue && distances[(k, j)] != int.MaxValue)
                {
                    // Ajout du temps de changement si nécessaire
                    int tempsChangement = 0;
                    if (ligneArrivee[(i, k)] != null && 
                        ligneArrivee[(k, j)] != null && 
                        ligneArrivee[(i, k)] != ligneArrivee[(k, j)])
                    {
                        tempsChangement = k.TempsChangement;
                    }

                    int newDist = distances[(i, k)] + distances[(k, j)] + tempsChangement;
                    if (newDist < distances[(i, j)])
                    {
                        distances[(i, j)] = newDist;
                        next[(i, j)] = next[(i, k)];
                        ligneArrivee[(i, j)] = ligneArrivee[(i, k)];
                    }
                }
            }
        }
    }

    return (distances, next, ligneArrivee);
}

        /// <summary>
        /// Implémente l'algorithme de Floyd-Warshall avec prise en compte des changements de ligne.
        /// </summary>
        /// <returns>Un tuple contenant la matrice des distances, la matrice des prédécesseurs et la matrice des lignes</returns>
        /// /// <summary>
        /// Récupère le plus court chemin entre deux stations à l'aide des matrices calculées par Floyd-Warshall
        /// </summary>
        /// <param name="source">Station de départ</param>
        /// <param name="target">Station d'arrivée</param>
        /// <param name="distances">Matrice des distances</param>
        /// <param name="next">Matrice des prédécesseurs</param>
        /// <returns>Un tuple contenant le chemin et le temps total</returns>
        public (List<Station> path, int totalTime) GetShortestPathFloydWarshall(
            Station source, Station target, 
            Dictionary<(Station, Station), int> distances, 
            Dictionary<(Station, Station), Station> next)
        {
            if (next[(source, target)] == null)
            {
                throw new InvalidOperationException("Il n'existe pas de chemin entre ces deux stations.");
            }

            var path = new List<Station>();
            path.Add(source);

            var current = source;
            while (current != target)
            {
                current = next[(current, target)];
                path.Add(current);
            }

            return (path, distances[(source, target)]);
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

        /// <summary>
        /// Recherche le chemin le plus confortable entre deux stations en minimisant les changements de ligne
        /// </summary>
        public (List<Station> path, int totalTime, int changements) GetComfortablePath(Station start, Station end)
        {
            if (start == null || end == null)
                throw new ArgumentNullException("Les stations de départ et d'arrivée doivent être spécifiées.");

            // Poids important pour les changements de ligne (équivalent à plusieurs minutes de trajet)
            const int PENALITE_CHANGEMENT_LIGNE = 8;

            // Dictionnaires pour stocker les informations de chemin
            var distances = new Dictionary<Station, int>();
            var ligneArrivee = new Dictionary<Station, string>();
            var predecesseurs = new Dictionary<Station, Station>();
            var nonVisitees = new HashSet<Station>();

            // Initialisation
            foreach (var lien in Liens)
            {
                if (!distances.ContainsKey(lien.Depart))
                {
                    distances[lien.Depart] = lien.Depart == start ? 0 : int.MaxValue;
                    nonVisitees.Add(lien.Depart);
                }
                if (!distances.ContainsKey(lien.Arrivee))
                {
                    distances[lien.Arrivee] = lien.Arrivee == start ? 0 : int.MaxValue;
                    nonVisitees.Add(lien.Arrivee);
                }
            }

            // Algorithme de Dijkstra modifié
            while (nonVisitees.Count > 0)
            {
                // Trouver la station non visitée avec la distance minimale
                Station courant = null;
                int minDistance = int.MaxValue;
                foreach (var station in nonVisitees)
                {
                    if (distances[station] < minDistance)
                    {
                        minDistance = distances[station];
                        courant = station;
                    }
                }

                if (courant == null || courant == end || distances[courant] == int.MaxValue)
                    break;

                nonVisitees.Remove(courant);

                // Explorer les voisins
                foreach (var lien in Liens.Where(l => l.Depart == courant))
                {
                    // Calculer la pénalité pour changement de ligne
                    int coutChangementLigne = 0;
                    if (ligneArrivee.ContainsKey(courant) && 
                        ligneArrivee[courant] != null && 
                        ligneArrivee[courant] != lien.Ligne)
                    {
                        coutChangementLigne = PENALITE_CHANGEMENT_LIGNE;
                        // Augmenter davantage la pénalité si la station n'est pas un point de correspondance officiel
                        if (courant.TempsChangement <= 0)
                            coutChangementLigne *= 2;
                    }

                    // Nouvelle distance avec pénalité
                    int newDist = distances[courant] + lien.Temps + coutChangementLigne;

                    // Mettre à jour si meilleur chemin
                    if (newDist < distances.GetValueOrDefault(lien.Arrivee, int.MaxValue))
                    {
                        distances[lien.Arrivee] = newDist;
                        predecesseurs[lien.Arrivee] = courant;
                        ligneArrivee[lien.Arrivee] = lien.Ligne;
                    }
                }
            }

            // Reconstruire le chemin
            var path = new List<Station>();
            int changements = 0;
            string lignePrecedente = null;
            
            if (predecesseurs.ContainsKey(end))
            {
                var courant = end;
                while (courant != null)
                {
                    path.Insert(0, courant);
                    
                    // Compter les changements de ligne
                    if (predecesseurs.ContainsKey(courant) && lignePrecedente != null && 
                        ligneArrivee.ContainsKey(predecesseurs[courant]) && 
                        ligneArrivee[predecesseurs[courant]] != lignePrecedente)
                    {
                        changements++;
                    }
                    
                    lignePrecedente = ligneArrivee.GetValueOrDefault(courant);
                    predecesseurs.TryGetValue(courant, out courant);
                }
            }
            else
            {
                throw new InvalidOperationException("Aucun chemin n'a été trouvé entre ces deux stations.");
            }

            // Calculer le temps total réel (avec les temps de changement inclus)
            int totalTime = 0;
            changements = 0;
            lignePrecedente = null;
            
            for (int i = 0; i < path.Count - 1; i++)
            {
                var lien = Liens.FirstOrDefault(l => l.Depart == path[i] && l.Arrivee == path[i + 1]);
                if (lien != null)
                {
                    totalTime += lien.Temps;
                    
                    // Ajouter le temps de changement si nécessaire
                    if (lignePrecedente != null && lignePrecedente != lien.Ligne)
                    {
                        changements++;
                        totalTime += path[i].TempsChangement > 0 ? path[i].TempsChangement : 2; // Par défaut 2 minutes si non spécifié
                    }
                    
                    lignePrecedente = lien.Ligne;
                }
            }

            return (path, totalTime, changements);
        }
    }
}