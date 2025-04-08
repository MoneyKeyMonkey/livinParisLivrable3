using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace LivinParisApp
{
    public class FormAdminLogin : Form
    {
        TextBox txtEmail, txtPassword;

        public FormAdminLogin()
        {
            // Configuration de la fenêtre
            this.Text = "Admin Login";
            this.Size = new System.Drawing.Size(300, 200);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Création des champs de texte
            Label lblEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(20, 20) };
            txtEmail = new TextBox { Location = new System.Drawing.Point(100, 20), Width = 150 };

            Label lblPassword = new Label { Text = "Mot de passe:", Location = new System.Drawing.Point(20, 60) };
            txtPassword = new TextBox { Location = new System.Drawing.Point(100, 60), Width = 150, PasswordChar = '*' };

            // Création du bouton de connexion
            Button btnLogin = new Button { Text = "Se connecter", Location = new System.Drawing.Point(100, 100) };
            btnLogin.Click += BtnLogin_Click;

            // Ajout des contrôles au formulaire
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(btnLogin);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
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

                        if (role == "admin")
                        {
                            MessageBox.Show("Connexion réussie en tant qu'admin.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Logique de redirection vers l'interface admin
                        }
                        else
                        {
                            MessageBox.Show("Accès refusé. Vous n'êtes pas un administrateur.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
