using System;
using System.Drawing;
using System.Windows.Forms;

// Assurez-vous d'ajouter les directives using nécessaires
namespace WinFormsApp1;// Remplacez 'YourNamespace' par le nom de votre espace de noms


public partial class GrapheForm : Form
{
    public GrapheForm()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Graphe";
        this.Size = new Size(800, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
    }
}
