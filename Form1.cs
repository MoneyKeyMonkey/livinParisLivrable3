using liv_inParis;
using System.Data;
using System.Text;

namespace LivinParisApp
{
    public partial class Form1 : Form
    {
        // Déclarez les variables comme membres de la classe
        private MetroDataService metroData;
        private List<Station> stations;
        private List<Lien> connexions;
        private List<Lien> connexionsOriginales; // Ajoutez cette ligne
        private Graphe graphe;
        private Station? stationSelectionnee;
        private Station? stationDepart;
        private Station? stationArrivee;
        private Lien? lienSelectionne; // Ajoutez cette ligne
        private Button btnAfficherStations;
        private Button btnAfficherLiens;
        private Button btnDijkstra;
        private Button btnAfficherTemps; // Ajoutez ce bouton
        private Button btnBellmanFord; // Ajoutez ce bouton
        private Button btnCouper; // Ajoutez ce bouton
        private Button btnReset; // Ajoutez ce bouton
        private Button btnHelp; // New button for help
        private Button btnColoration; // Ajoutez ce bouton à la liste des membres
        private TextBox txtRechercheStation;
        private ListBox lstStations;
        public int rayon = 10;

        // Ajoutez cette variable membre pour suivre l'état du mode découpage
        private bool modeCoupage = false;

        public Form1()
        {
            InitializeComponent();

            // Créez une instance de MetroDataService
            metroData = new MetroDataService();

            // Accédez aux propriétés via l'instance metroData
            stations = metroData.Stations;  // Au lieu de MetroDataService.Stations
            connexions = metroData.Connexions;  // Au lieu de MetroDataService.Connexions
            connexionsOriginales = new List<Lien>(connexions); // Sauvegardez les connexions originales
                                                               // Créez une instance de Graphe
            graphe = new Graphe(connexions);

            // Abonnez-vous à l'événement Paint
            this.Paint += new PaintEventHandler(Form1_Paint);
            this.MouseClick += new MouseEventHandler(Form1_MouseClick);

            // Ajoutez les contrôles
            btnAfficherStations = new Button { Text = "Afficher les Stations", Location = new Point(10, 10), Width = 150 };
            btnAfficherLiens = new Button { Text = "Afficher les Liens", Location = new Point(10, 50), Width = 150 };
            btnAfficherTemps = new Button { Text = "Afficher les Temps", Location = new Point(10, 130), Width = 150 };
            btnDijkstra = new Button { Text = "Dijkstra", Location = new Point(10, 170) };
            btnBellmanFord = new Button { Text = "Bellman-Ford", Location = new Point(10, 210) };
            btnCouper = new Button { Text = "Couper", Location = new Point(10, 250) };
            btnReset = new Button { Text = "Reset", Location = new Point(10, 290) };
            btnColoration = new Button { Text = "Coloration de graphe", Location = new Point(10, 330), Width = 150 };
            btnHelp = new Button { Text = "MODE D'EMPLOI", Location = new Point(10, 370), Width = 150 };
            txtRechercheStation = new TextBox { Location = new Point(10, 410), Width = 200 };
            lstStations = new ListBox { Location = new Point(10, 450), Width = 200, Height = 100 };

            // Ajoute les boutons cliquables
            btnAfficherStations.Click += BtnAfficherStations_Click;
            btnAfficherLiens.Click += BtnAfficherLiens_Click;
            btnAfficherTemps.Click += BtnAfficherTemps_Click;
            btnDijkstra.Click += BtnDijkstra_Click;
            btnBellmanFord.Click += BtnBellmanFord_Click;
            btnCouper.Click += BtnCouper_Click;
            btnReset.Click += BtnReset_Click;
            btnColoration.Click += BtnColoration_Click;
            btnHelp.Click += BtnHelp_Click;
            txtRechercheStation.TextChanged += TxtRechercheStation_TextChanged;
            lstStations.MouseDoubleClick += LstStations_MouseDoubleClick;

            this.Controls.Add(btnAfficherStations);
            this.Controls.Add(btnAfficherLiens);
            this.Controls.Add(btnAfficherTemps);
            this.Controls.Add(btnDijkstra);
            this.Controls.Add(btnBellmanFord);
            this.Controls.Add(btnCouper);
            this.Controls.Add(btnReset);
            this.Controls.Add(btnColoration); // Ajoutez le bouton au formulaire
            this.Controls.Add(btnHelp);
            this.Controls.Add(txtRechercheStation);
            this.Controls.Add(lstStations);

            //plein écran
            this.WindowState = FormWindowState.Maximized;

            // Pre-fill the search bar
            txtRechercheStation.Text = "Rechercher une station";
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Permet de dessiner les stations et les connexions sur le formulaire.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            // Dessinez les stations et les connexions
            graphe.AfficherStations(e.Graphics, stations, stationSelectionnee);
            graphe.AfficherConnexions(e.Graphics, stations);

            if (stationDepart != null && stationArrivee != null)
            {
                var (path, totalTime) = graphe.GetShortestPath(stationDepart, stationArrivee);
                graphe.AfficherChemin(e.Graphics, path, Color.Red); // Ajoutez la couleur ici
            }
        }

        /// <summary>
        /// Permet de sélectionner une station ou un lien en cliquant dessus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (modeCoupage)
            {
                // En mode découpage, on supprime directement les éléments cliqués
                bool elementSupprime = false;

                // Vérifier d'abord les stations
                foreach (var station in stations.ToList())
                {
                    int hitboxRayon = (int)(rayon * 1.5);
                    var rect = new Rectangle(station.Position.X - hitboxRayon, station.Position.Y - hitboxRayon,
                                            2 * hitboxRayon, 2 * hitboxRayon);

                    if (rect.Contains(e.Location))
                    {
                        // Supprimer tous les liens connectés à cette station
                        var liensASupprimer = connexions.Where(l => l.Depart == station || l.Arrivee == station).ToList();

                        foreach (var lien in liensASupprimer)
                        {
                            connexions.Remove(lien);
                        }

                        // Nettoyer les références à cette station
                        if (stationDepart == station) stationDepart = null;
                        if (stationArrivee == station) stationArrivee = null;
                        if (stationSelectionnee == station) stationSelectionnee = null;

                        elementSupprime = true;
                        break;
                    }
                }

                // Si aucune station n'a été supprimée, vérifier les liens
                if (!elementSupprime)
                {
                    foreach (var lien in connexions.ToList())
                    {
                        if (LienClique(lien, e.Location))
                        {
                            // Supprimer le lien cliqué
                            connexions.Remove(lien);
                            
                            // Trouver et supprimer également le lien inverse (si existant)
                            var lienInverse = connexions.FirstOrDefault(l => 
                                l.Depart == lien.Arrivee && l.Arrivee == lien.Depart);
                            
                            if (lienInverse != null)
                            {
                                connexions.Remove(lienInverse);
                            }
                            
                            // Mettre à jour les références
                            if (lienSelectionne == lien || lienSelectionne == lienInverse) 
                                lienSelectionne = null;
                            
                            elementSupprime = true;
                            break;
                        }
                    }
                }

                // Si un élément a été supprimé, mettre à jour le graphe UNE SEULE FOIS
                if (elementSupprime)
                {
                    graphe = new Graphe(connexions);
                    this.Refresh(); // Force un rafraîchissement immédiat
                }
            }
            else
            {
                // Comportement normal (sélection de stations/liens)

                // Vérifiez si une station a été cliquée
                foreach (var station in stations)
                {
                    int hitboxRayon = (int)(rayon * 1.5);
                    var rect = new Rectangle(station.Position.X - hitboxRayon, station.Position.Y - hitboxRayon,
                                            2 * hitboxRayon, 2 * hitboxRayon);

                    if (rect.Contains(e.Location))
                    {
                        if (stationDepart == null)
                        {
                            stationDepart = station;
                        }
                        else if (stationArrivee == null)
                        {
                            stationArrivee = station;
                        }
                        else
                        {
                            stationDepart = station;
                            stationArrivee = null;
                        }

                        stationSelectionnee = station;
                        lienSelectionne = null;
                        this.Invalidate();
                        return;
                    }
                }

                // Si aucune station n'est sélectionnée, vérifiez les liens
                foreach (var lien in connexions)
                {
                    if (LienClique(lien, e.Location))
                    {
                        lienSelectionne = lien;
                        stationSelectionnee = null;
                        this.Invalidate();
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Vérifie si un lien a été cliqué.
        /// </summary>
        private bool LienClique(Lien lien, Point point)
        {
            if (lien?.Depart?.Position == null || lien?.Arrivee?.Position == null)
                return false;

            // MODIFICATION 3: Réduire la hitbox des liens
            int rayonLien = 3; // Réduire la sensibilité des liens (au lieu de 6)
            var p1 = AjusterPoint(lien.Depart.Position, lien.Arrivee.Position, rayon);
            var p2 = AjusterPoint(lien.Arrivee.Position, lien.Depart.Position, rayon);
            var distance = DistancePointALigne(p1, p2, point);

            // MODIFICATION 4: Réduire le seuil de détection
            return distance < 4; // Réduire la sensibilité (au lieu de 5)
        }

        private Point AjusterPoint(Point p1, Point p2, int rayon)
        {
            double angle = Math.Atan2(p2.Y - p1.Y, p2.X - p1.X);
            return new Point((int)(p1.X + rayon * Math.Cos(angle)), (int)(p1.Y + rayon * Math.Sin(angle)));
        }

        /// <summary>
        /// Calcule la distance entre un point et une ligne.
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        private double DistancePointALigne(Point p1, Point p2, Point p)
        {
            double A = p.X - p1.X;
            double B = p.Y - p1.Y;
            double C = p2.X - p1.X;
            double D = p2.Y - p1.Y;

            double dot = A * C + B * D;
            double len_sq = C * C + D * D;
            double param = dot / len_sq;

            double xx, yy;

            if (param < 0 || (p1.X == p2.X && p1.Y == p2.Y))
            {
                xx = p1.X;
                yy = p1.Y;
            }
            else if (param > 1)
            {
                xx = p2.X;
                yy = p2.Y;
            }
            else
            {
                xx = p1.X + param * C;
                yy = p1.Y + param * D;
            }

            double dx = p.X - xx;
            double dy = p.Y - yy;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Affiche la liste des stations dans une boîte de dialogue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAfficherStations_Click(object sender, EventArgs e)
        {
            metroData.AfficherStations();
        }

        /// <summary>
        /// Affiche la liste des connexions dans une boîte de dialogue.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAfficherLiens_Click(object sender, EventArgs e)
        {
            metroData.AfficherLiens();
        }

        /// <summary>
        /// Recherche le chemin le plus court entre la station de départ et la station d'arrivée.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnDijkstra_Click(object sender, EventArgs e)
        {
            if (stationDepart != null && stationArrivee != null)
            {
                var (path, totalTime) = graphe.GetShortestPath(stationDepart, stationArrivee);

                // Afficher le résultat ici
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Temps de trajet total : {totalTime} minutes");
                sb.AppendLine("Stations empruntées :");
                sb.AppendLine(string.Join(" --> ", path.Select(s => s.Nom)));
                MessageBox.Show(sb.ToString(), "Chemin le plus court");

                this.Invalidate(); // Redessiner le formulaire pour afficher le chemin
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station de départ et une station d'arrivée.");
            }
        }

        /// <summary>
        /// Permet d'afficher ou de masquer les temps de trajet sur les connexions.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnAfficherTemps_Click(object sender, EventArgs e)
        {
            graphe.AfficherTemps = !graphe.AfficherTemps;
            this.Invalidate(); // Redessiner le formulaire pour afficher/masquer les temps de trajet
        }

        /// <summary>
        /// Recherche le chemin le plus court entre la station de départ et la station d'arrivée en utilisant l'algorithme Bellman-Ford.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnBellmanFord_Click(object sender, EventArgs e)
        {
            if (stationDepart != null && stationArrivee != null)
            {
                try
                {
                    var (path, totalTime) = graphe.GetShortestPathBellmanFord(stationDepart, stationArrivee);

                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine($"Temps de trajet total : {totalTime} minutes");
                    sb.AppendLine("Stations empruntées :");
                    sb.AppendLine(string.Join(" --> ", path.Select(s => s.Nom)));
                    MessageBox.Show(sb.ToString(), "Chemin Bellman-Ford");

                    graphe.AfficherChemin(this.CreateGraphics(), path, Color.Blue); // Utilisez la couleur bleue pour Bellman-Ford

                    this.Invalidate();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Erreur");
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station de départ et d'arrivée.");
            }
        }

        /// <summary>
        /// Permet de filtrer les stations par nom.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TxtRechercheStation_TextChanged(object sender, EventArgs e)
        {
            string recherche = txtRechercheStation.Text.ToLower();
            var resultats = stations.Where(s => s.Nom.ToLower().StartsWith(recherche)).ToList();
            lstStations.DataSource = resultats;
            lstStations.DisplayMember = "Nom";
        }

        /// <summary>
        /// Permet de sélectionner une station en double-cliquant dessus dans le menu déroulant.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LstStations_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstStations.SelectedItem is Station station)
            {
                if (stationDepart == null)
                {
                    stationDepart = station;
                }
                else if (stationArrivee == null)
                {
                    stationArrivee = station;
                }
                else
                {
                    stationDepart = station;
                    stationArrivee = null;
                }

                stationSelectionnee = station;
                this.Invalidate(); // Redessiner le formulaire pour mettre à jour la couleur de la station sélectionnée
            }
        }

        private void BtnHelp_Click(object sender, EventArgs e)
        {
            // Show a message box with instructions on how to interact with the graph
            MessageBox.Show("Instructions:\n\n1. Recherchez ou sélectionnez directement les stations sur le graphe pour trouver le plus court chemin.\n2. Sélectionnez un lien et coupez-le pour trouver un chemin alternatif.\n3. Utilisez les boutons pour plus de renseignements.");
        }

        private void BtnCouper_Click(object sender, EventArgs e)
        {
            // Activer/désactiver le mode découpage
            modeCoupage = !modeCoupage;

            if (modeCoupage)
            {
                // Changer l'apparence du bouton pour indiquer que le mode est actif
                btnCouper.BackColor = Color.Red;
                btnCouper.Text = "Mode Découpage (Actif)";
                MessageBox.Show("Mode découpage activé. Cliquez sur une station ou un lien pour le supprimer.",
                    "Mode découpage", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                // Restaurer l'apparence du bouton
                btnCouper.BackColor = SystemColors.Control;
                btnCouper.Text = "Couper";
            }

            // Réinitialiser les sélections pour éviter les conflits
            stationSelectionnee = null;
            lienSelectionne = null;
            this.Invalidate();
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            connexions = new List<Lien>(connexionsOriginales); // Réinitialisez les connexions
            graphe = new Graphe(connexions); // Recréez le graphe avec les connexions réinitialisées
            lienSelectionne = null;
            this.Invalidate(); // Redessiner le formulaire pour mettre à jour les connexions
        }

        /// <summary>
        /// Active ou désactive la coloration de graphe
        /// </summary>
        private void BtnColoration_Click(object sender, EventArgs e)
        {
            graphe.ModeColoration = !graphe.ModeColoration;

            if (graphe.ModeColoration)
            {
                // Lancer l'algorithme de coloration
                var coloration = graphe.ColorerStations();

                // Compter le nombre de couleurs utilisées
                int nombreCouleurs = coloration.Values.Max() + 1;
                MessageBox.Show($"Coloration effectuée avec {nombreCouleurs} couleurs.\n\nLes stations adjacentes ont des couleurs différentes.", "Coloration de graphe");

                btnColoration.Text = "Désactiver coloration";
            }
            else
            {
                btnColoration.Text = "Coloration de graphe";
            }

            this.Invalidate(); // Redessiner le formulaire pour actualiser les couleurs
        }
    }
}
