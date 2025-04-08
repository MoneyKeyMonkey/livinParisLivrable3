//using MySql.Data.MySqlClient;
//using System;
//using System.Windows.Forms;

//namespace WinFormsApp1
//{
//    public class FormConnect : Form
//    {
//        private Label lblEmail;
//        private TextBox txtEmail;
//        private Label lblPassword;
//        private TextBox txtPassword;
//        private Button btnLogin;
//        private Button btnCancel;

//        public FormConnect()
//        {
//            InitializeComponent();
//        }

//        private void InitializeComponent()
//        {
//            this.Text = "Connexion - LivinParisApp";
//            this.Size = new System.Drawing.Size(400, 250);
//            this.StartPosition = FormStartPosition.CenterScreen;

//            lblEmail = new Label { Text = "Email :", Left = 50, Top = 30, Width = 80 };
//            txtEmail = new TextBox { Left = 140, Top = 30, Width = 200 };

//            lblPassword = new Label { Text = "Mot de passe :", Left = 50, Top = 70, Width = 80 };
//            txtPassword = new TextBox { Left = 140, Top = 70, Width = 200, PasswordChar = '*' };

//            btnLogin = new Button { Text = "Connexion", Left = 140, Top = 110, Width = 100 };
//            btnLogin.Click += BtnLogin_Click;

//            btnCancel = new Button { Text = "Annuler", Left = 250, Top = 110, Width = 90 };
//            btnCancel.Click += (sender, e) => { this.Close(); };

//            this.Controls.Add(lblEmail);
//            this.Controls.Add(txtEmail);
//            this.Controls.Add(lblPassword);
//            this.Controls.Add(txtPassword);
//            this.Controls.Add(btnLogin);
//            this.Controls.Add(btnCancel);
//        }

//        private void BtnLogin_Click(object sender, EventArgs e)
//        {
//            if (string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
//            {
//                MessageBox.Show("Veuillez remplir tous les champs.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                return;
//            }

//            string connectionString = "Server=localhost;Database=livin_paris;User ID=root;Password=root";
//            using (MySqlConnection connection = new MySqlConnection(connectionString))
//            {
//                try
//                {
//                    connection.Open();

//                    string query = "SELECT id_utilisateur FROM utilisateur WHERE email = @Email AND mdp = SHA2(@MotDePasse, 256)";

//                    using (MySqlCommand cmd = new MySqlCommand(query, connection))
//                    {
//                        cmd.Parameters.AddWithValue("@Email", txtEmail.Text);
//                        cmd.Parameters.AddWithValue("@MotDePasse", txtPassword.Text);

//                        object result = cmd.ExecuteScalar();

//                        if (result != null)
//                        {
//                            MessageBox.Show("Connexion réussie !", "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
//                            this.DialogResult = DialogResult.OK;
//                            this.Close();
//                        }
//                        else
//                        {
//                            MessageBox.Show("Email ou mot de passe incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                        }
//                    }
//                }
//                catch (Exception ex)
//                {
//                    MessageBox.Show("Une erreur est survenue : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
//                }
//            }
//        }
//    }
//}