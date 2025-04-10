using MySql.Data.MySqlClient;
using System;
using System.Windows.Forms;

namespace LivinParisApp
{
    public class FormModifyUser : Form
    {
        TextBox txtPrenom, txtNom, txtEmail, txtNumTel, txtAdresse, txtPassword;
        ComboBox cmbRole;
        Button btnSave;
        string userId;

        public FormModifyUser(string userId)
        {
            this.userId = userId;

            // Configuration de la fen�tre
            this.Text = "Modifier un utilisateur";
            this.Size = new System.Drawing.Size(400, 400);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Cr�ation des champs de texte
            Label lblPrenom = new Label { Text = "Pr�nom:", Location = new System.Drawing.Point(20, 20) };
            txtPrenom = new TextBox { Location = new System.Drawing.Point(120, 20), Width = 200 };

            Label lblNom = new Label { Text = "Nom:", Location = new System.Drawing.Point(20, 60) };
            txtNom = new TextBox { Location = new System.Drawing.Point(120, 60), Width = 200 };

            Label lblEmail = new Label { Text = "Email:", Location = new System.Drawing.Point(20, 100) };
            txtEmail = new TextBox { Location = new System.Drawing.Point(120, 100), Width = 200 };

            Label lblNumTel = new Label { Text = "Num�ro de t�l�phone:", Location = new System.Drawing.Point(20, 140) };
            txtNumTel = new TextBox { Location = new System.Drawing.Point(120, 140), Width = 200 };

            Label lblAdresse = new Label { Text = "Adresse:", Location = new System.Drawing.Point(20, 180) };
            txtAdresse = new TextBox { Location = new System.Drawing.Point(120, 180), Width = 200 };

            Label lblPassword = new Label { Text = "Mot de passe:", Location = new System.Drawing.Point(20, 220) };
            txtPassword = new TextBox { Location = new System.Drawing.Point(120, 220), Width = 200, PasswordChar = '*' };

            Label lblRole = new Label { Text = "R�le:", Location = new System.Drawing.Point(20, 260) };
            cmbRole = new ComboBox { Location = new System.Drawing.Point(120, 260), Width = 200 };
            cmbRole.Items.AddRange(new string[] { "client", "entreprise", "cuisinier" });

            // Cr�ation du bouton de sauvegarde
            btnSave = new Button { Text = "Sauvegarder", Location = new System.Drawing.Point(120, 300), Width = 200 };
            btnSave.Click += BtnSave_Click;

            // Ajout des contr�les au formulaire
            this.Controls.Add(lblPrenom);
            this.Controls.Add(txtPrenom);
            this.Controls.Add(lblNom);
            this.Controls.Add(txtNom);
            this.Controls.Add(lblEmail);
            this.Controls.Add(txtEmail);
            this.Controls.Add(lblNumTel);
            this.Controls.Add(txtNumTel);
            this.Controls.Add(lblAdresse);
            this.Controls.Add(txtAdresse);
            this.Controls.Add(lblPassword);
            this.Controls.Add(txtPassword);
            this.Controls.Add(lblRole);
            this.Controls.Add(cmbRole);
            this.Controls.Add(btnSave);

            LoadUserData();
        }

        private void LoadUserData()
        {
            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT prenom, nom, email, num_tel, adresse, role FROM utilisateur WHERE id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtPrenom.Text = reader["prenom"].ToString();
                                txtNom.Text = reader["nom"].ToString();
                                txtEmail.Text = reader["email"].ToString();
                                txtNumTel.Text = reader["num_tel"].ToString();
                                txtAdresse.Text = reader["adresse"].ToString();
                                cmbRole.SelectedItem = reader["role"].ToString();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ...existing code...
        private void BtnSave_Click(object sender, EventArgs e)
        {
            string prenom = txtPrenom.Text;
            string nom = txtNom.Text;
            string email = txtEmail.Text;
            string numTel = txtNumTel.Text;
            string adresse = txtAdresse.Text;
            string password = txtPassword.Text;
            string role = cmbRole.SelectedItem.ToString();

            if (string.IsNullOrWhiteSpace(prenom) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(role))
            {
                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "UPDATE utilisateur SET prenom = @Prenom, nom = @Nom, email = @Email, num_tel = @NumTel, adresse = @Adresse, role = @Role" +
                                   (string.IsNullOrWhiteSpace(password) ? "" : ", mdp = SHA2(@Password, 256)") +
                                   " WHERE id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@Prenom", prenom);
                        cmd.Parameters.AddWithValue("@Nom", nom);
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@NumTel", numTel);
                        cmd.Parameters.AddWithValue("@Adresse", adresse);
                        cmd.Parameters.AddWithValue("@Role", role);
                        if (!string.IsNullOrWhiteSpace(password))
                        {
                            cmd.Parameters.AddWithValue("@Password", password);
                        }
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Utilisateur modifi� avec succ�s.", "Succ�s", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
