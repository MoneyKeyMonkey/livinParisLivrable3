using MySql.Data.MySqlClient;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace LivinParisApp
{
    public class FormSeConnecter : Form
    {
        TextBox txtEmail, txtPassword;

        public FormSeConnecter()
        {
            // Configuration de la fenêtre
            this.Text = "Se Connecter";
            this.Size = new System.Drawing.Size(350, 250);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            // Création des champs de texte
            Label lblEmail = new Label { Text = "Email:", Location = new Point(20, 20), Font = new Font("Arial", 12, FontStyle.Regular) };
            txtEmail = new TextBox { Location = new Point(120, 20), Width = 200, Font = new Font("Arial", 12, FontStyle.Regular) };
            txtEmail.PlaceholderText = "Entrez votre email";

            Label lblPassword = new Label { Text = "Mot de passe:", Location = new Point(20, 60), Font = new Font("Arial", 12, FontStyle.Regular) };
            txtPassword = new TextBox { Location = new Point(120, 60), Width = 200, Font = new Font("Arial", 12, FontStyle.Regular), PasswordChar = '*' };
            txtPassword.PlaceholderText = "Entrez votre mot de passe";

            // Création des boutons
            Button btnAdmin = CreateStyledButton("Admin", new Point(20, 100));
            btnAdmin.Click += BtnAdmin_Click;

            Button btnClient = CreateStyledButton("Client", new Point(200, 100));
            btnClient.Click += BtnClient_Click;

            Button btnCuisinier = CreateStyledButton("Cuisinier", new Point(20, 140));
            btnCuisinier.Click += BtnCuisinier_Click;

            Button btnEntreprise = CreateStyledButton("Entreprise", new Point(200, 140));
            btnEntreprise.Click += BtnEntreprise_Click;

            // Ajout des contrôles au formulaire
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnAdmin);
            this.Controls.Add(btnClient);
            this.Controls.Add(btnCuisinier);
            this.Controls.Add(btnEntreprise);
        }

        private Button CreateStyledButton(string text, Point location)
        {
            Button button = new Button();
            button.Text = text;
            button.Size = new Size(120, 30);
            button.Location = location;
            button.BackColor = Color.RoyalBlue;
            button.ForeColor = Color.White;
            button.Font = new Font("Arial", 12, FontStyle.Bold);
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = Color.DarkBlue;
            return button;
        }

        private void BtnAdmin_Click(object sender, EventArgs e)
        {
            AuthenticateAdmin();
        }

        private void BtnClient_Click(object sender, EventArgs e)
        {
            AuthenticateUser("client");
        }

        private void BtnCuisinier_Click(object sender, EventArgs e)
        {
            AuthenticateUser("cuisinier");
        }

        private void BtnEntreprise_Click(object sender, EventArgs e)
        {
            AuthenticateUser("entreprise");
        }

        private void AuthenticateAdmin()
        {
            string email = txtEmail.Text;
            string password = txtPassword.Text;

            if (email == "root" && password == "root")
            {
                MessageBox.Show("Connexion réussie en tant qu'admin.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Ouvrir la fenêtre de liste des utilisateurs
                FormUserList formUserList = new FormUserList();
                formUserList.Show();
            }
            else
            {
                MessageBox.Show("Accès refusé. Email ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AuthenticateUser(string expectedRole)
        {
            string email = txtEmail.Text;
            string password = txtPassword.Text;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Veuillez entrer votre email et mot de passe.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT role FROM utilisateur WHERE email = @Email AND mdp = SHA2(@MotDePasse, 256)";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@MotDePasse", password);

                        var role = cmd.ExecuteScalar() as string;

                        if (role == expectedRole)
                        {
                            MessageBox.Show($"Connexion réussie en tant que {expectedRole}.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Logique de redirection vers l'interface appropriée
                        }
                        else
                        {
                            MessageBox.Show($"Accès refusé. Vous n'êtes pas un {expectedRole}.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}