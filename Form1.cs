using liv_inParis;
using System.Data;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
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
        private Button btnFloydWarshall; // Ajoutez ce bouton
        private TextBox txtRechercheStation;
        private ListBox lstStations;
        public int rayon = 10;

        // Ajoutez cette variable membre pour suivre l'état du mode découpage
        private bool modeCoupage = false;

        // Ajoutez ces variables membres à votre classe Form1
        private float zoomFactor = 1.0f;
        private Point panOffset = Point.Empty;
        private bool isPanning = false;
        private Point lastMousePosition;

        private Panel topPanel;
        private Panel leftPanel;
        private Panel statusPanel;
        private GroupBox searchGroup;
        private GroupBox algorithmsGroup;
        private GroupBox toolsGroup;
        private GroupBox displayGroup;
        private Label statusLabel;

        // Constantes et structures pour les messages tactiles Windows
        private const int WM_GESTURE = 0x0119;
        private const int WM_GESTURENOTIFY = 0x011A;
        private const int GC_ALLGESTURES = 0x00000001;
        private const int GC_ZOOM = 0x00000010;
        private const int GC_PAN = 0x00000001;
        private const int GID_ZOOM = 3;
        private const int GID_PAN = 1;
        private const int GF_BEGIN = 0x00000001;
        private const int GF_INERTIA = 0x00000002;
        private const int GF_END = 0x00000004;

        [StructLayout(LayoutKind.Sequential)]
        private struct GESTUREINFO
        {
            public uint cbSize;
            public uint dwFlags;
            public uint dwID;
            public IntPtr hwndTarget;
            public POINTS ptsLocation;
            public uint dwInstanceID;
            public uint dwSequenceID;
            public ulong ullArguments;
            public uint cbExtraArgs;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINTS
        {
            public short x;
            public short y;
        }

        // P/Invoke déclarations pour accéder aux APIs Windows
        [DllImport("user32.dll")]
        private static extern bool SetGestureConfig(
            IntPtr hWnd,
            uint dwReserved,
            uint cIDs,
            GESTURECONFIG[] pGestureConfig,
            uint cbSize);

        [DllImport("user32.dll")]
        private static extern bool GetGestureInfo(
            IntPtr hGestureInfo,
            ref GESTUREINFO pGestureInfo);

        [DllImport("user32.dll")]
        private static extern bool CloseGestureInfoHandle(
            IntPtr hGestureInfo);

        [StructLayout(LayoutKind.Sequential)]
        private struct GESTURECONFIG
        {
            public uint dwID;
            public uint dwWant;
            public uint dwBlock;
        }

        // Variables pour suivre les gestes
        private float initialDistance = 0;
        private float baseZoomFactor = 1.0f;

        public Form1()
        {
            InitializeComponent();
            
            // Active le double buffering pour une animation fluide
            this.SetStyle(
                ControlStyles.AllPaintingInWmPaint | 
                ControlStyles.UserPaint | 
                ControlStyles.DoubleBuffer |
                ControlStyles.OptimizedDoubleBuffer,
                true);
            
            // Configuration du formulaire pour les gestes de touchpad
            this.KeyPreview = true;
            this.SetStyle(ControlStyles.StandardClick | 
                        ControlStyles.StandardDoubleClick | 
                        ControlStyles.Selectable, true);
            this.TabStop = true;
            
            // Configuration du formulaire
            this.Text = "Métro Paris - Simulation de trajets";
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.WhiteSmoke; // Ajouter une icône si disponible
            
            // Créer les panneaux de contrôles
            InitializeControlPanels();
            InitializeControls();
            LoadData();
            
            // Abonnements aux événements
            this.Paint += new PaintEventHandler(Form1_Paint);
            this.MouseClick += new MouseEventHandler(Form1_MouseClick);
            this.Resize += Form1_Resize;
            this.MouseWheel += Form1_MouseWheel; // Ajouter cet événement pour le zoom
            this.MouseDown += Form1_MouseDown;   // Ajouter pour le début du déplacement
            this.MouseUp += Form1_MouseUp;       // Ajouter pour la fin du déplacement
            this.MouseMove += Form1_MouseMove;   // Ajouter pour le déplacement
            
            // Pré-remplir la barre de recherche
            txtRechercheStation.Text = "Rechercher une station...";
            txtRechercheStation.ForeColor = Color.Gray;
            txtRechercheStation.GotFocus += (s, e) => { 
                if (txtRechercheStation.Text == "Rechercher une station...") {
                    txtRechercheStation.Text = "";
                    txtRechercheStation.ForeColor = Color.Black;
                }
            };
            txtRechercheStation.LostFocus += (s, e) => { 
                if (string.IsNullOrEmpty(txtRechercheStation.Text)) {
                    txtRechercheStation.Text = "Rechercher une station...";
                    txtRechercheStation.ForeColor = Color.Gray;
                }
            };

            // Activez le support des gestes tactiles
            this.HandleCreated += Form1_HandleCreated;
        }

        private void Form1_HandleCreated(object sender, EventArgs e)
        {
            // Configurer les gestes que nous voulons recevoir
            GESTURECONFIG[] gc = new GESTURECONFIG[2];
            
            // Configurer le zoom (pincement/écartement)
            gc[0].dwID = GID_ZOOM;
            gc[0].dwWant = 1;
            gc[0].dwBlock = 0;
            
            // Configurer le déplacement (pan)
            gc[1].dwID = GID_PAN;
            gc[1].dwWant = 1;
            gc[1].dwBlock = 0;

            bool result = SetGestureConfig(
                this.Handle,
                0,
                (uint)gc.Length,
                gc,
                (uint)Marshal.SizeOf(typeof(GESTURECONFIG)));
        }

        protected override void WndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_GESTURENOTIFY:
                    // Indique à Windows que nous acceptons les gestes
                    base.WndProc(ref m);
                    break;
                
                case WM_GESTURE:
                    // Traiter le geste
                    this.DecodeGesture(m.LParam);
                    m.Result = (IntPtr)1; // Marquer comme traité
                    break;
                
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        private void DecodeGesture(IntPtr gestureInfoPtr)
        {
            GESTUREINFO gi = new GESTUREINFO();
            gi.cbSize = (uint)Marshal.SizeOf(typeof(GESTUREINFO));

            if (!GetGestureInfo(gestureInfoPtr, ref gi))
                return;

            switch (gi.dwID)
            {
                case GID_ZOOM:
                    // Geste de zoom (pincement/écartement)
                    if ((gi.dwFlags & GF_BEGIN) != 0)
                    {
                        // Début du geste de zoom
                        initialDistance = (float)Math.Sqrt((double)gi.ullArguments);
                        baseZoomFactor = zoomFactor;
                    }
                    else
                    {
                        // Zoom en cours
                        float currentDistance = (float)Math.Sqrt((double)gi.ullArguments);
                        
                        // Calculer le nouveau facteur de zoom avec une sensibilité contrôlée
                        float ratio = currentDistance / initialDistance;
                        float newZoom = baseZoomFactor * ratio;
                        
                        // Limiter le zoom et appliquer avec une marge pour les mouvements
                        // (un petit mouvement ne changera pas beaucoup le zoom)
                        newZoom = Math.Max(0.3f, Math.Min(5.0f, newZoom));
                        
                        if (Math.Abs(zoomFactor - newZoom) > 0.01f) // Marge pour les petits mouvements
                        {
                            zoomFactor = newZoom;
                            
                            // Point central du geste
                            Point center = new Point(gi.ptsLocation.x, gi.ptsLocation.y);
                            
                            // Mise à jour du statut et redessinage
                            UpdateStatusMessage($"Zoom: {zoomFactor:F2}x");
                            this.Invalidate();
                        }
                    }
                    break;
                
                case GID_PAN:
                    // Geste de déplacement à deux doigts
                    Point currentPoint = new Point(gi.ptsLocation.x, gi.ptsLocation.y);
                    
                    if ((gi.dwFlags & GF_BEGIN) != 0)
                    {
                        // Début du déplacement
                        lastMousePosition = currentPoint;
                        isPanning = true;
                    }
                    else if ((gi.dwFlags & GF_END) != 0)
                    {
                        // Fin du déplacement
                        isPanning = false;
                    }
                    else
                    {
                        // Déplacement en cours
                        int deltaX = currentPoint.X - lastMousePosition.X;
                        int deltaY = currentPoint.Y - lastMousePosition.Y;

                        // Mettre à jour la position de décalage
                        panOffset.X += deltaX;
                        panOffset.Y += deltaY;

                        // Mettre à jour la dernière position connue
                        lastMousePosition = currentPoint;

                        // Redessiner
                        this.Invalidate();
                    }
                    break;
            }

            CloseGestureInfoHandle(gestureInfoPtr);
        }

        private void InitializeControlPanels()
        {
            // Panneau supérieur pour le titre et informations générales
            topPanel = new Panel {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(30, 45, 75) // Bleu foncé élégant
            };
            
            // Logo et titre
            Label titleLabel = new Label {
                Text = "PLAN DU MÉTRO PARISIEN",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(20, 15)
            };
            topPanel.Controls.Add(titleLabel);
            
            // Panneau gauche pour les contrôles
            leftPanel = new Panel {
                Dock = DockStyle.Left,
                Width = 250,
                BackColor = Color.FromArgb(240, 240, 240),
                Padding = new Padding(10)
            };
            
            // Panneau de statut
            statusPanel = new Panel {
                Dock = DockStyle.Bottom,
                Height = 25,
                BackColor = Color.FromArgb(220, 220, 220)
            };
            
            statusLabel = new Label {
                AutoSize = true,
                Location = new Point(10, 5),
                Text = "Prêt"
            };
            statusPanel.Controls.Add(statusLabel);
            
            // Ajout des panneaux au formulaire
            this.Controls.Add(topPanel);
            this.Controls.Add(leftPanel);
            this.Controls.Add(statusPanel);
            
            // Création des groupes dans le panneau gauche
            searchGroup = new GroupBox {
                Text = "Recherche de stations",
                Dock = DockStyle.Top,
                Height = 180,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            algorithmsGroup = new GroupBox {
                Text = "Algorithmes de recherche",
                Dock = DockStyle.Top,
                Height = 140,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            toolsGroup = new GroupBox {
                Text = "Outils d'édition",
                Dock = DockStyle.Top,
                Height = 100,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            displayGroup = new GroupBox {
                Text = "Options d'affichage",
                Dock = DockStyle.Top,
                Height = 120,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            leftPanel.Controls.Add(searchGroup);
            leftPanel.Controls.Add(algorithmsGroup);
            leftPanel.Controls.Add(toolsGroup);
            leftPanel.Controls.Add(displayGroup);
        }

        private void InitializeControls()
        {
            // Configuration des contrôles de recherche
            txtRechercheStation = new TextBox {
                Width = 210,
                Height = 25,
                Location = new Point(15, 30),
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9)
            };
            
            lstStations = new ListBox {
                Width = 210,
                Height = 100,
                Location = new Point(15, 65),
                Font = new Font("Segoe UI", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            searchGroup.Controls.Add(txtRechercheStation);
            searchGroup.Controls.Add(lstStations);
            
            // Configuration des boutons d'algorithmes (sans icônes)
            btnDijkstra = CreateStyledButton("Dijkstra", null, new Point(15, 25), 100);
            btnBellmanFord = CreateStyledButton("Bellman-Ford", null, new Point(125, 25), 100);
            btnFloydWarshall = CreateStyledButton("Floyd-Warshall", null, new Point(15, 70), 210);
            Button btnConfort = CreateStyledButton("Trajet Confort", null, new Point(15, 115), 210);
            
            algorithmsGroup.Controls.Add(btnDijkstra);
            algorithmsGroup.Controls.Add(btnBellmanFord);
            algorithmsGroup.Controls.Add(btnFloydWarshall);
            algorithmsGroup.Controls.Add(btnConfort);
            algorithmsGroup.Height += 55; // Augmenter la hauteur pour le nouveau bouton
            
            // Configuration des boutons d'outils (sans icônes)
            btnCouper = CreateStyledButton("Couper", null, new Point(15, 25), 100);
            btnReset = CreateStyledButton("Réinitialiser", null, new Point(125, 25), 100);
            
            toolsGroup.Controls.Add(btnCouper);
            toolsGroup.Controls.Add(btnReset);
            
            // Configuration des boutons d'affichage (sans icônes)
            btnAfficherStations = CreateStyledButton("Stations", null, new Point(15, 25), 100);
            btnAfficherLiens = CreateStyledButton("Liens", null, new Point(125, 25), 100);
            btnAfficherTemps = CreateStyledButton("Temps", null, new Point(15, 70), 100);
            btnColoration = CreateStyledButton("Coloration", null, new Point(125, 70), 100);
            Button btnResetView = CreateStyledButton("Réinit. Vue", null, new Point(15, 115), 210);
            
            displayGroup.Controls.Add(btnAfficherStations);
            displayGroup.Controls.Add(btnAfficherLiens);
            displayGroup.Controls.Add(btnAfficherTemps);
            displayGroup.Controls.Add(btnColoration);
            displayGroup.Controls.Add(btnResetView);
            displayGroup.Height += 45; // Augmenter la hauteur pour le nouveau bouton
            
            // Bouton d'aide en bas du panneau gauche
            btnHelp = new Button {
                Text = "AIDE",
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                Location = new Point(15, leftPanel.Height - 60),
                Width = 210,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 45, 75),
                ForeColor = Color.White,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };
            
            leftPanel.Controls.Add(btnHelp);
            
            // Ajouter les gestionnaires d'événements
            btnDijkstra.Click += BtnDijkstra_Click;
            btnBellmanFord.Click += BtnBellmanFord_Click;
            btnFloydWarshall.Click += BtnFloydWarshall_Click;
            btnCouper.Click += BtnCouper_Click;
            btnReset.Click += BtnReset_Click;
            btnAfficherStations.Click += BtnAfficherStations_Click;
            btnAfficherLiens.Click += BtnAfficherLiens_Click;
            btnAfficherTemps.Click += BtnAfficherTemps_Click;
            btnColoration.Click += BtnColoration_Click;
            btnHelp.Click += BtnHelp_Click;
            txtRechercheStation.TextChanged += TxtRechercheStation_TextChanged;
            lstStations.MouseDoubleClick += LstStations_MouseDoubleClick;
            btnResetView.Click += BtnResetView_Click;
            btnConfort.Click += BtnConfort_Click;
        }

        private Button CreateStyledButton(string text, Image icon, Point location, int width)
        {
            Button btn = new Button {
                Text = text,
                Font = new Font("Segoe UI", 9),
                Location = location,
                Width = width,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                TextAlign = ContentAlignment.MiddleRight,
                TextImageRelation = TextImageRelation.ImageBeforeText,
                ImageAlign = ContentAlignment.MiddleLeft,
                FlatAppearance = { BorderColor = Color.FromArgb(200, 200, 200), BorderSize = 1 }
            };
            
            if (icon != null)
                btn.Image = icon;
                
            return btn;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Centrer le graphe
            CentrerGraphe();
            
            // Initialiser la liste des stations
            lstStations.DataSource = stations;
            lstStations.DisplayMember = "Nom";
        }

        /// <summary>
        /// Permet de dessiner les stations et les connexions sur le formulaire.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            using (BufferedGraphics bufferedGraphics = BufferedGraphicsManager.Current.Allocate(
                e.Graphics, Rectangle.Round(e.Graphics.VisibleClipBounds)))
            {
                Graphics g = bufferedGraphics.Graphics;
                g.Clear(this.BackColor);
                
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                
                Matrix originalTransform = g.Transform;
                
                try
                {
                    // Appliquer les transformations de zoom et déplacement
                    g.TranslateTransform(panOffset.X, panOffset.Y);
                    g.ScaleTransform(zoomFactor, zoomFactor);
                    
                    // Dessiner les stations sans les noms
                    graphe.AfficherStations(g, stations, stationSelectionnee, false);
                    graphe.AfficherConnexions(g, stations);

                    if (stationDepart != null && stationArrivee != null)
                    {
                        var (path, totalTime) = graphe.GetShortestPath(stationDepart, stationArrivee);
                        graphe.AfficherChemin(g, path, Color.Red);
                    }
                }
                finally
                {
                    // Restaurer l'état initial des graphiques
                    g.Transform = originalTransform;
                    
                    // Maintenant dessiner les noms des stations à taille fixe
                    graphe.AfficherNomsStations(g, stations, panOffset, zoomFactor);
                }
                
                bufferedGraphics.Render();
            }
        }

        /// <summary>
        /// Permet de sélectionner une station ou un lien en cliquant dessus.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (isPanning) return;
            
            // Convertir les coordonnées du clic en coordonnées dans l'espace transformé
            Point transformedPoint = new Point(
                (int)((e.X - panOffset.X) / zoomFactor),
                (int)((e.Y - panOffset.Y) / zoomFactor)
            );
            
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

                    if (rect.Contains(transformedPoint))
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
                        
                        // Afficher l'info
                        MessageBox.Show($"Station {station.Nom} et {liensASupprimer.Count} liens ont été supprimés.", 
                            "Suppression effectuée", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        
                        elementSupprime = true;
                        break;
                    }
                }

                // Si aucune station n'a été supprimée, vérifier les liens
                if (!elementSupprime)
                {
                    foreach (var lien in connexions.ToList())
                    {
                        if (LienClique(lien, transformedPoint))
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
                            
                            MessageBox.Show($"Lien entre {lien.Depart.Nom} et {lien.Arrivee.Nom} supprimé dans les deux sens.",
                                "Suppression effectuée", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            
                            elementSupprime = true;
                            break;
                        }
                    }
                }

                // Si un élément a été supprimé, mettre à jour le graphe
                if (elementSupprime)
                {
                    graphe = new Graphe(connexions);
                    this.Refresh();
                }
            }
            else
            {
                // Comportement normal (sélection de stations/liens)
                bool stationTrouvee = false;
                
                foreach (var station in stations)
                {
                    int hitboxRayon = (int)(rayon * 1.5);
                    var rect = new Rectangle(station.Position.X - hitboxRayon, station.Position.Y - hitboxRayon,
                                            2 * hitboxRayon, 2 * hitboxRayon);

                    if (rect.Contains(transformedPoint))
                    {
                        if (stationDepart == null)
                        {
                            stationDepart = station;
                            UpdateStatusMessage($"Station de départ : {station.Nom}");
                        }
                        else if (stationArrivee == null)
                        {
                            stationArrivee = station;
                            UpdateStatusMessage($"Station d'arrivée : {station.Nom}");
                        }
                        else
                        {
                            stationDepart = station;
                            stationArrivee = null;
                            UpdateStatusMessage($"Nouvelle station de départ : {station.Nom}");
                        }

                        stationSelectionnee = station;
                        stationTrouvee = true;
                        this.Invalidate();
                        break;
                    }
                }

                // Si aucune station n'est sélectionnée, vérifier les liens
                if (!stationTrouvee)
                {
                    foreach (var lien in connexions)
                    {
                        if (LienClique(lien, transformedPoint))
                        {
                            lienSelectionne = lien;
                            stationSelectionnee = null;
                            UpdateStatusMessage($"Lien sélectionné : {lien.Depart.Nom} → {lien.Arrivee.Nom}");
                            this.Invalidate();
                            break;
                        }
                    }
                }
            }
        }

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
        /// <param sender="sender"></param>
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

                // Utiliser la nouvelle méthode d'affichage standardisée
                var (message, changements) = FormatPathDisplay(path, totalTime, connexions);

                MessageBox.Show(message, "Algorithme Dijkstra", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Afficher le chemin sur le graphe
                graphe.AfficherChemin(this.CreateGraphics(), path, Color.Red);
                this.Invalidate();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station de départ et une station d'arrivée.", 
                               "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    // Utiliser la nouvelle méthode d'affichage standardisée
                    var (message, changements) = FormatPathDisplay(path, totalTime, connexions);

                    MessageBox.Show(message, "Algorithme Bellman-Ford", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Afficher le chemin sur le graphe avec une couleur distincte
                    graphe.AfficherChemin(this.CreateGraphics(), path, Color.Blue);
                    this.Invalidate();
                }
                catch (InvalidOperationException ex)
                {
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station de départ et une station d'arrivée.", 
                               "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        /// <summary>
        /// Recherche le chemin le plus court entre la station de départ et la station d'arrivée en utilisant l'algorithme Floyd-Warshall.
        /// </summary>
        private void BtnFloydWarshall_Click(object sender, EventArgs e)
        {
            if (stationDepart != null && stationArrivee != null)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    // Calcul des matrices avec Floyd-Warshall amélioré
                    var (distances, next, ligneArrivee) = graphe.FloydWarshallAvecChangements();
                    var (path, totalTime) = graphe.GetShortestPathFloydWarshall(stationDepart, stationArrivee, distances, next);

                    Cursor = Cursors.Default;


                    // Utiliser la nouvelle méthode d'affichage standardisée
                    var (message, changements) = FormatPathDisplay(path, totalTime, connexions);

                    MessageBox.Show(message, "Algorithme Floyd-Warshall", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Afficher le chemin sur le graphe avec une couleur distincte
                    graphe.AfficherChemin(this.CreateGraphics(), path, Color.Purple);
                    this.Invalidate();
                }
                catch (InvalidOperationException ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station de départ et une station d'arrivée.", 
                              "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
            
            // Si la recherche est vide ou contient le texte par défaut, afficher toutes les stations
            if (string.IsNullOrWhiteSpace(recherche) || recherche == "rechercher une station...")
            {
                lstStations.DataSource = stations;
            }
            else
            {
                // Utiliser Contains au lieu de StartsWith pour des résultats plus complets
                var resultats = stations.Where(s => s.Nom.ToLower().Contains(recherche)).ToList();
                lstStations.DataSource = resultats;
            }
            
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
            MessageBox.Show(
                "Instructions:\n\n" +
                "1. Recherchez ou sélectionnez directement les stations sur le graphe pour trouver le plus court chemin.\n" +
                "2. Sélectionnez un lien et coupez-le pour trouver un chemin alternatif.\n" +
                "3. Utilisez la molette de souris ou le geste de pincement sur le pavé tactile pour zoomer.\n" +
                "4. Maintenez le bouton droit ou du milieu de la souris pour déplacer la carte.\n" +
                "5. Utilisez le bouton \"Réinit. Vue\" pour revenir à la vue normale.",
                "Aide", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnCouper_Click(object sender, EventArgs e)
        {
            modeCoupage = !modeCoupage;
            
            if (modeCoupage)
            {
                btnCouper.BackColor = Color.FromArgb(255, 200, 200); // Rouge clair
                btnCouper.Text = "Arrêter";
                UpdateStatusMessage("Mode découpage actif: cliquez sur une station ou un lien pour le supprimer");
            }
            else
            {
                btnCouper.BackColor = SystemColors.Control;
                btnCouper.Text = "Couper";
                UpdateStatusMessage("Mode découpage désactivé");
            }
            
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
                var coloration = graphe.ColorerStations();
                int nombreCouleurs = coloration.Values.Max() + 1;
                
                btnColoration.BackColor = Color.FromArgb(200, 230, 200); // Vert clair
                btnColoration.Text = "Standard";
                UpdateStatusMessage($"Coloration avec {nombreCouleurs} couleurs");
            }
            else
            {
                btnColoration.BackColor = SystemColors.Control;
                btnColoration.Text = "Coloration";
                UpdateStatusMessage("Mode d'affichage standard");
            }
            
            this.Invalidate();
        }

        private void Form1_Resize(object sender, EventArgs e)
{
    btnHelp.Location = new Point(15, leftPanel.ClientSize.Height - 60);
    
    // Recalculer le centrage quand la fenêtre est redimensionnée
    if (stations != null && stations.Count > 0)
    {
        CentrerGraphe();
    }
}

        private void UpdateStatusMessage(string message, bool isError = false)
        {
            statusLabel.Text = message;
            statusLabel.ForeColor = isError ? Color.Red : Color.Black;
        }



        private void LoadData()
{
    // Initialisation des données du métro
    metroData = new MetroDataService();
    stations = metroData.Stations;
    connexions = metroData.Connexions;
    connexionsOriginales = new List<Lien>(connexions);
    graphe = new Graphe(connexions);
    
    // Ne pas utiliser BeginInvoke ici
    // this.BeginInvoke(new Action(() => {
    //     CentrerGraphe();
    // }));
}

        // Gestionnaire pour la molette de la souris et gestes de pincement touchpad
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            float oldZoom = zoomFactor;
            
            // Récupérer les modificateurs de touches (Ctrl, Shift, etc.)
            bool isCtrlPressed = (ModifierKeys & Keys.Control) == Keys.Control;
            
            // Augmenter le facteur de zoom pour une réponse plus rapide
            float zoomStep = isCtrlPressed ? 1.3f : 1.15f; // Valeurs plus élevées pour un zoom plus rapide
            
            // Calculer le point central de la zone d'affichage
            Point centreAffichage = new Point(
                leftPanel.Width + (this.ClientSize.Width - leftPanel.Width) / 2,
                this.ClientSize.Height / 2
            );
            
            // Appliquer le zoom directement
            if (e.Delta > 0)
                zoomFactor *= zoomStep;
            else
                zoomFactor /= zoomStep;
            
            // Le reste du code reste identique...
            
            // Limiter le facteur de zoom
            zoomFactor = Math.Max(0.3f, Math.Min(5.0f, zoomFactor));
            
            // Option 1: Ajustement centré sur la souris (plus naturel pour petits ajustements)
            if (isNearClick(e.Location, centreAffichage, 200)) // Si près du centre
            {
                // Centrage par rapport au centre de la zone d'affichage
                panOffset.X = centreAffichage.X - (int)((centreAffichage.X - panOffset.X) * (zoomFactor / oldZoom));
                panOffset.Y = centreAffichage.Y - (int)((centreAffichage.Y - panOffset.Y) * (zoomFactor / oldZoom));
            }
            else
            {
                // Centrage par rapport à la position de la souris (pour les zoom ciblés)
                Point mousePos = e.Location;
                panOffset.X = mousePos.X - (int)((mousePos.X - panOffset.X) * (zoomFactor / oldZoom));
                panOffset.Y = mousePos.Y - (int)((mousePos.Y - panOffset.Y) * (zoomFactor / oldZoom));
            }

            // Mettre à jour le statut
            UpdateStatusMessage($"Zoom: {zoomFactor:F2}x");
            
            // Redessiner immédiatement
            this.Invalidate();
        }

        // Méthode auxiliaire pour déterminer si un point est proche d'un autre
        private bool isNearClick(Point p1, Point p2, int maxDistance)
        {
            int dx = p1.X - p2.X;
            int dy = p1.Y - p2.Y;
            return (dx * dx + dy * dy) <= maxDistance * maxDistance;
        }

        // Gestionnaires pour le déplacement (pan)
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                isPanning = true;
                lastMousePosition = e.Location;
                this.Cursor = Cursors.SizeAll;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (isPanning && (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right))
            {
                isPanning = false;
                this.Cursor = Cursors.Default;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isPanning)
            {
                // Calculer le déplacement
                int deltaX = e.X - lastMousePosition.X;
                int deltaY = e.Y - lastMousePosition.Y;

                // Mettre à jour la position de décalage
                panOffset.X += deltaX;
                panOffset.Y += deltaY;

                // Mettre à jour la dernière position connue
                lastMousePosition = e.Location;

                // Redessiner
                this.Invalidate();
            }
        }

        private void BtnResetView_Click(object sender, EventArgs e)
        {
            zoomFactor = 1.0f;
            CentrerGraphe(); // Utiliser la méthode de centrage existante au lieu de simplement réinitialiser panOffset
            UpdateStatusMessage("Vue réinitialisée");
            this.Invalidate();
        }

        private void CentrerGraphe()
        {
            if (stations == null || stations.Count == 0) return;

            // Calculer les limites du graphe
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var station in stations)
            {
                if (station.Position == null) continue;
                
                if (station.Position.X < minX) minX = station.Position.X;
                if (station.Position.Y < minY) minY = station.Position.Y;
                if (station.Position.X > maxX) maxX = station.Position.X;
                if (station.Position.Y > maxY) maxY = station.Position.Y;
            }

            // Calculer le centre du graphe
            int grapheCentreX = (minX + maxX) / 2;
            int grapheCentreY = (minY + maxY) / 2;

            // Calculer le centre de la zone visible (à droite du panneau gauche)
            // Corrigé pour vraiment utiliser le centre de la zone d'affichage
            int zoneAffichageX = leftPanel.Width + (this.ClientSize.Width - leftPanel.Width) / 2;
            int zoneAffichageY = this.ClientSize.Height / 2;

            // Définir le décalage pour centrer
            panOffset.X = zoneAffichageX - grapheCentreX;
            panOffset.Y = zoneAffichageY - grapheCentreY;

            // Mettre à jour l'affichage
            this.Invalidate();
        }

        /// <summary>
        /// Recherche le chemin le plus confortable (minimisant les changements de ligne)
        /// </summary>
        private void BtnConfort_Click(object sender, EventArgs e)
        {
            if (stationDepart != null && stationArrivee != null)
            {
                try
                {
                    Cursor = Cursors.WaitCursor;

                    var (path, totalTime, changements) = graphe.GetComfortablePath(stationDepart, stationArrivee);

                    Cursor = Cursors.Default;

                    // Utiliser la nouvelle méthode d'affichage standardisée (mais utiliser le nombre de changements déjà calculé)
                    var (message, _) = FormatPathDisplay(path, totalTime, connexions);

                    MessageBox.Show(message, "Trajet Confort", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Afficher le chemin sur le graphe avec une couleur distincte (vert)
                    graphe.AfficherChemin(this.CreateGraphics(), path, Color.FromArgb(0, 180, 0));
                    this.Invalidate();
                }
                catch (InvalidOperationException ex)
                {
                    Cursor = Cursors.Default;
                    MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner une station de départ et une station d'arrivée.", 
                               "Sélection requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private (string message, int changements) FormatPathDisplay(List<Station> path, int totalTime, List<Lien> connexionsLocales)
        {
            StringBuilder sb = new StringBuilder();
            int changements = 0;
            string ligneCourante = null;
            
            // Compter les changements et construire le message
            for (int i = 0; i < path.Count; i++)
            {
                if (i < path.Count - 1)
                {
                    var lien = connexionsLocales.FirstOrDefault(l => l.Depart == path[i] && l.Arrivee == path[i + 1]);
                    string nouvelleLigne = lien?.Ligne ?? "?";
                    
                    if (ligneCourante != null && ligneCourante != nouvelleLigne)
                    {
                        changements++;
                        // Changement de ligne
                        sb.AppendLine($"{path[i].Nom} [Changer pour ligne {nouvelleLigne}]");
                    }
                    else
                    {
                        sb.AppendLine(path[i].Nom);
                    }
                    
                    ligneCourante = nouvelleLigne;
                }
                else
                {
                    // Dernière station
                    sb.AppendLine(path[i].Nom);
                }
            }
            
            string headerMessage = $"✓ Trajet avec {changements} changement(s)\n✓ Temps de trajet total : {totalTime} minutes\n\nStations empruntées :";
            
            return (headerMessage + "\n" + sb.ToString(), changements);
        }
    }
}
