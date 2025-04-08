using System;
using System.Drawing;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace LivinParisApp
{
    public class MainForm : Form
    {
        private MySqlConnection connection;

       public MainForm()
{
    // Configuration de la fenêtre principale
    this.Text = "LivinParis";
    this.WindowState = FormWindowState.Maximized;
    this.BackColor = Color.White;
    this.Padding = new Padding(20);

    // Création du label titre
    Label titleLabel = new Label();
    titleLabel.Text = "LIVINPARIS";
    titleLabel.Font = new Font("Arial", 36, FontStyle.Bold);
    titleLabel.ForeColor = Color.FromArgb(255, 87, 34); // Leboncoin orange color
    titleLabel.AutoSize = true;
    titleLabel.Location = new Point((this.ClientSize.Width - titleLabel.Width) / 2 -90, 50);
    titleLabel.Anchor = AnchorStyles.Top;
    this.Controls.Add(titleLabel);

    // Création du FlowLayoutPanel pour les boutons à gauche
    FlowLayoutPanel leftButtonPanel = new FlowLayoutPanel();
    leftButtonPanel.FlowDirection = FlowDirection.LeftToRight;
    leftButtonPanel.AutoSize = true;
    leftButtonPanel.Location = new Point((this.ClientSize.Width - titleLabel.Width) / 2 - 800, titleLabel.Location.Y + (titleLabel.Height / 2) - 40);
    leftButtonPanel.Anchor = AnchorStyles.Top;
    this.Controls.Add(leftButtonPanel);

    // Création du FlowLayoutPanel pour les boutons à droite
    FlowLayoutPanel rightButtonPanel = new FlowLayoutPanel();
    rightButtonPanel.FlowDirection = FlowDirection.LeftToRight;
    rightButtonPanel.AutoSize = true;
    rightButtonPanel.Location = new Point((this.ClientSize.Width + titleLabel.Width) / 2 + 280, titleLabel.Location.Y + (titleLabel.Height / 2) - 40);
    rightButtonPanel.Anchor = AnchorStyles.Top;
    this.Controls.Add(rightButtonPanel);

    // Bouton Se connecter
    Button btnConnect = CreateStyledButton("Se connecter", Color.FromArgb(255, 87, 34)); // Leboncoin orange color
    btnConnect.Click += (sender, e) => { new FormSeConnecter().Show(); };
    leftButtonPanel.Controls.Add(btnConnect);

    // Bouton S'inscrire
    Button btnInscrire = CreateStyledButton("S'inscrire", Color.FromArgb(255, 87, 34)); // Leboncoin orange color
    btnInscrire.Click += (sender, e) => { new FormInscrire().Show(); };
    leftButtonPanel.Controls.Add(btnInscrire);

    // Bouton Plan Metro Paris
    Button btnPlanMetro = CreateStyledButton("Plan Metro Paris", Color.FromArgb(255, 87, 34)); // Leboncoin orange color
    btnPlanMetro.Click += (sender, e) => { new Form1().Show(); };
    rightButtonPanel.Controls.Add(btnPlanMetro);

    // Bouton Se Déconnecter
    Button btnDeconnecter = CreateStyledButton("Se Déconnecter", Color.FromArgb(255, 87, 34)); // Leboncoin orange color
    btnDeconnecter.Click += BtnDeconnecter_Click;
    rightButtonPanel.Controls.Add(btnDeconnecter);

    // Initialiser la connexion à la base de données
    string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
    connection = new MySqlConnection(connectionString);
    connection.Open();
}




        private Button CreateStyledButton(string text, Color backColor)
        {
            Button button = new Button();
            button.Text = text;
            button.BackColor = backColor;
            button.ForeColor = Color.White;
            button.Font = new Font("Arial", 14, FontStyle.Bold);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(230, 81, 0); // Darker orange for hover effect
            button.Size = new Size(250, 80); // Agrandir les boutons
            return button;
        }



        private void BtnDeconnecter_Click(object sender, EventArgs e)
        {
            // Logique de déconnexion de l'utilisateur
            if (connection != null && connection.State == System.Data.ConnectionState.Open)
            {
                connection.Close();
                MessageBox.Show("Utilisateur déconnecté de la base de données.", "Déconnexion", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Aucune connexion active à la base de données.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
