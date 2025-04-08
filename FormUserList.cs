using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows.Forms;

namespace LivinParisApp
{
    public class FormUserList : Form
    {
        Button btnClientEntrepriseMenu, btnCuisinierMenu, btnCommandeMenu, btnDeleteUser, btnCreateUser, btnGenerateUsers, btnChangeRole, btnRefresh; DataGridView dataGridView;

        public FormUserList()
        {
            // Configuration de la fenêtre 
            this.Text = "Menu Principal";
            this.Size = new System.Drawing.Size(800, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Création du TableLayoutPanel 
            TableLayoutPanel tableLayoutPanel = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 5,
                Dock = DockStyle.Fill,
                Padding = new Padding(10),
                AutoSize = true
            };
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33F));
            tableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34F));

            // Création des boutons 
            btnClientEntrepriseMenu = new Button { Text = "Menu Client/Entreprise", Anchor = AnchorStyles.None, Width = 200 };
            btnClientEntrepriseMenu.Click += BtnClientEntrepriseMenu_Click;

            btnCuisinierMenu = new Button { Text = "Menu Cuisinier", Anchor = AnchorStyles.None, Width = 200 };
            btnCuisinierMenu.Click += BtnCuisinierMenu_Click;

            btnCommandeMenu = new Button { Text = "Menu Commande", Anchor = AnchorStyles.None, Width = 200 };
            btnCommandeMenu.Click += BtnCommandeMenu_Click;

            // Création du DataGridView 
            dataGridView = new DataGridView { Dock = DockStyle.Fill, Visible = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false, ReadOnly = false };
            dataGridView.CellEndEdit += DataGridView_CellEndEdit;

            // Création des boutons de suppression, de création, de génération et de rafraîchissement 
            btnDeleteUser = new Button { Text = "Supprimer l'utilisateur", Anchor = AnchorStyles.None, Width = 200, Visible = false };
            btnDeleteUser.Click += BtnDeleteUser_Click;

            btnCreateUser = new Button { Text = "Créer un utilisateur", Anchor = AnchorStyles.None, Width = 200, Visible = false };
            btnCreateUser.Click += BtnCreateUser_Click;

            btnGenerateUsers = new Button { Text = "Générer 10 utilisateurs", Anchor = AnchorStyles.None, Width = 200, Visible = false };
            btnGenerateUsers.Click += BtnGenerateUsers_Click;

            btnChangeRole = new Button { Text = "Changer le rôle", Anchor = AnchorStyles.None, Width = 200, Visible = false };
            btnChangeRole.Click += BtnChangeRole_Click;

            btnRefresh = new Button { Text = "Rafraîchir", Anchor = AnchorStyles.None, Width = 200, Visible = false };
            btnRefresh.Click += BtnRefresh_Click;

            // Ajout des boutons et du DataGridView au TableLayoutPanel 
            tableLayoutPanel.Controls.Add(btnClientEntrepriseMenu, 0, 0);
            tableLayoutPanel.Controls.Add(btnCuisinierMenu, 1, 0);
            tableLayoutPanel.Controls.Add(btnCommandeMenu, 2, 0);
            tableLayoutPanel.Controls.Add(dataGridView, 0, 1);
            tableLayoutPanel.SetColumnSpan(dataGridView, 3);
            tableLayoutPanel.Controls.Add(btnDeleteUser, 0, 2);
            tableLayoutPanel.Controls.Add(btnCreateUser, 1, 2);
            tableLayoutPanel.Controls.Add(btnGenerateUsers, 2, 2);
            tableLayoutPanel.Controls.Add(btnChangeRole, 0, 3);
            tableLayoutPanel.Controls.Add(btnRefresh, 1, 3);

            // Ajout du TableLayoutPanel au formulaire 
            this.Controls.Add(tableLayoutPanel);
        }

        private void BtnClientEntrepriseMenu_Click(object sender, EventArgs e)
        {
            // Afficher le DataGridView et les boutons de suppression, de création, de génération, de changement de rôle et de rafraîchissement 
            dataGridView.Visible = true;
            btnDeleteUser.Visible = true;
            btnCreateUser.Visible = true;
            btnGenerateUsers.Visible = true;
            btnChangeRole.Visible = true;
            btnRefresh.Visible = true;

            // Charger les données des utilisateurs avec le rôle "client" ou "entreprise" 
            LoadClientEntrepriseData();
        }

        private void BtnCuisinierMenu_Click(object sender, EventArgs e)
        {
            // Afficher le DataGridView et les boutons de suppression, de création, de génération, de changement de rôle et de rafraîchissement 
            dataGridView.Visible = true;
            btnDeleteUser.Visible = true;
            btnCreateUser.Visible = true;
            btnGenerateUsers.Visible = true;
            btnChangeRole.Visible = true;
            btnRefresh.Visible = true;

            // Charger les données des utilisateurs avec le rôle "cuisinier" 
            LoadCuisinierData();
        }

        private void BtnCommandeMenu_Click(object sender, EventArgs e)
        {
            // Afficher le DataGridView et les boutons de rafraîchissement 
            dataGridView.Visible = true;
            btnDeleteUser.Visible = false;
            btnCreateUser.Visible = false;
            btnGenerateUsers.Visible = false;
            btnChangeRole.Visible = false;
            btnRefresh.Visible = true;

            // Charger les données des commandes 
            LoadCommandeData();
        }

        private void LoadClientEntrepriseData()
        {
            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT id, prenom, nom, email, num_tel, adresse, role FROM utilisateur WHERE role = 'client' OR role = 'entreprise'";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadCuisinierData()
        {
            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT id, prenom, nom, email, num_tel, adresse, role FROM utilisateur WHERE role = 'cuisinier'";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadCommandeData()
        {
            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT id_commande, id_client, id_cuisinier, date_commande, statut FROM commande";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        dataGridView.DataSource = dataTable;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                string userId = dataGridView.SelectedRows[0].Cells["id"].Value.ToString();

                string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();

                        string query = "DELETE FROM utilisateur WHERE id = @UserId";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Utilisateur supprimé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Refresh the data grid view based on the current menu 
                        RefreshDataGrid();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur à supprimer.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnCreateUser_Click(object sender, EventArgs e)
        {
            // Ouvrir une nouvelle fenêtre pour créer un utilisateur 
            FormUser formCreateUser = new FormUser(false);
            formCreateUser.ShowDialog();

            // Refresh the data grid view based on the current menu 
            RefreshDataGrid();
        }

        private void BtnGenerateUsers_Click(object sender, EventArgs e)
        {
            // Générer 10 utilisateurs aléatoires 
            GenerateRandomUsers();

            // Refresh the data grid view based on the current menu 
            RefreshDataGrid();
        }

        private void GenerateRandomUsers()
        {
            string[] firstNames = { "John", "Jane", "Alex", "Emily", "Michael", "Sarah", "David", "Laura", "Robert", "Linda" };
            string[] lastNames = { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            string[] roles = { "client", "entreprise", "cuisinier" };

            Random random = new Random();
            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    for (int i = 0; i < 10; i++)
                    {
                        string firstName = firstNames[random.Next(firstNames.Length)];
                        string lastName = lastNames[random.Next(lastNames.Length)];
                        string email = $"{firstName.ToLower()}.{lastName.ToLower()}{random.Next(1000)}@example.com";
                        string numTel = $"06{random.Next(10000000, 99999999)}";
                        string adresse = $"{random.Next(1, 100)} Rue de {lastNames[random.Next(lastNames.Length)]}";
                        string password = "password";
                        string role = roles[random.Next(roles.Length)];

                        string query = "INSERT INTO utilisateur (id, prenom, nom, email, num_tel, adresse, mdp, role) " +
                                       "VALUES (UUID(), @Prenom, @Nom, @Email, @NumTel, @Adresse, SHA2(@Password, 256), @Role)";
                        using (MySqlCommand cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@Prenom", firstName);
                            cmd.Parameters.AddWithValue("@Nom", lastName);
                            cmd.Parameters.AddWithValue("@Email", email);
                            cmd.Parameters.AddWithValue("@NumTel", numTel);
                            cmd.Parameters.AddWithValue("@Adresse", adresse);
                            cmd.Parameters.AddWithValue("@Password", password);
                            cmd.Parameters.AddWithValue("@Role", role);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    MessageBox.Show("10 utilisateurs aléatoires générés avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnChangeRole_Click(object sender, EventArgs e)
        {
            if (dataGridView.SelectedRows.Count > 0)
            {
                string userId = dataGridView.SelectedRows[0].Cells["id"].Value.ToString();
                string currentRole = dataGridView.SelectedRows[0].Cells["role"].Value.ToString();

                // Create a new form to select the new role 
                Form selectRoleForm = new Form
                {
                    Text = "Sélectionner un nouveau rôle",
                    Size = new System.Drawing.Size(300, 200),
                    StartPosition = FormStartPosition.CenterScreen
                };

                ComboBox cmbNewRole = new ComboBox { Location = new System.Drawing.Point(50, 50), Width = 200 };
                cmbNewRole.Items.AddRange(new string[] { "client", "entreprise", "cuisinier" });
                cmbNewRole.SelectedItem = currentRole;

                Button btnSaveRole = new Button { Text = "Sauvegarder", Location = new System.Drawing.Point(100, 100), Width = 100 };
                btnSaveRole.Click += (s, args) =>
                {
                    string newRole = cmbNewRole.SelectedItem.ToString();

                    string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
                    using (MySqlConnection connection = new MySqlConnection(connectionString))
                    {
                        try
                        {
                            connection.Open();

                            string query = "UPDATE utilisateur SET role = @NewRole WHERE id = @UserId";
                            using (MySqlCommand cmd = new MySqlCommand(query, connection))
                            {
                                cmd.Parameters.AddWithValue("@NewRole", newRole);
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Rôle de l'utilisateur changé avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            // Refresh the data grid view based on the current menu 
                            RefreshDataGrid();

                            selectRoleForm.Close();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                };

                selectRoleForm.Controls.Add(cmbNewRole);
                selectRoleForm.Controls.Add(btnSaveRole);
                selectRoleForm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Veuillez sélectionner un utilisateur pour changer son rôle.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // Rafraîchir les données du DataGridView en fonction du menu actuellement sélectionné 
            RefreshDataGrid();
        }

        private void RefreshDataGrid()
        {
            if (btnClientEntrepriseMenu.Focused)
            {
                LoadClientEntrepriseData();
            }
            else if (btnCuisinierMenu.Focused)
            {
                LoadCuisinierData();
            }
            else if (btnCommandeMenu.Focused)
            {
                LoadCommandeData();
            }
            else
            {
                // Si aucun menu spécifique n'est sélectionné, recharger les données par défaut 
                LoadClientEntrepriseData();
            }
        }

        private void DataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            // Récupérer les nouvelles valeurs de la cellule éditée 
            string columnName = dataGridView.Columns[e.ColumnIndex].Name;
            string newValue = dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value.ToString();
            string userId = dataGridView.Rows[e.RowIndex].Cells["id"].Value.ToString();

            // Mettre à jour la base de données avec les nouvelles valeurs 
            string connectionString = "Server=localhost;Database=food_delivery;User ID=root;Password=root";
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    string query = $"UPDATE utilisateur SET {columnName} = @NewValue WHERE id = @UserId";
                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@NewValue", newValue);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        cmd.ExecuteNonQuery();
                    }

                    MessageBox.Show("Utilisateur mis à jour avec succès.", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
