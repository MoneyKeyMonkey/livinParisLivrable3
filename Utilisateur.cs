// Modèle Utilisateur
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using WinFormsApp1;

public class Utilisateur
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public string Prenom { get; set; }
    public string Email { get; set; }
    public string Telephone { get; set; }
    public string MotDePasse { get; set; }
    public string Type { get; set; }
    public bool EstActif { get; set; }
}
