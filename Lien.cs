

public class Lien
{

    public Station Depart { get; set; }
    public Station Arrivee { get; set; }
    public int Temps { get; set; }
    public bool Unidir { get; set; }
    public string Ligne { get; set; } // La ligne est maintenant dans Lien

    /// <summary>
    /// Initialise une nouvelle instance de la classe Lien avec les valeurs spécifiées.
    /// </summary>
    /// <param name="depart"></param>
    /// <param name="arrivee"></param>
    /// <param name="temps"></param>
    /// <param name="unidir"></param>
    /// <param name="ligne"></param>
    public Lien(Station depart, Station arrivee, int temps, bool unidir, string ligne)
    {
        Depart = depart;
        Arrivee = arrivee;
        Temps = temps;
        Unidir = unidir;
        Ligne = ligne;
    }
}