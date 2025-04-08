public class Station
{
    public int Id { get; set; }
    public string Nom { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public Point Position { get; set; }
    public List<Station> StationsAdjacentes { get; set; } = new();
    public int TempsChangement { get; set; }

    /// <summary>
    /// Initialise une nouvelle instance de la classe Station avec les valeurs spécifiées.
    /// </summary>
    /// <param name="id">L'identifiant de la station.</param>
    /// <param name="nom">Le nom de la station.</param>
    /// <param name="latitude">La latitude de la station.</param>
    /// <param name="longitude">La longitude de la station.</param>
    public Station(int id, string nom, float latitude, float longitude)
    {
        Id = id;
        Nom = nom;
        Latitude = latitude;
        Longitude = longitude;
        TempsChangement = 5;
    }

    /// <summary>
    /// Ajoute une station adjacente à la liste des stations adjacentes.
    /// </summary>
    /// <param name="station">La station adjacente à ajouter.</param>
    public void AjouterStationAdjacente(Station station)
    {
        if (station != null && station.Id != this.Id)
            StationsAdjacentes.Add(station);
    }

    /// <summary>
    /// Retourne une chaîne qui représente l'objet actuel.
    /// </summary>
    /// <returns>Le nom de la station.</returns>
    public override string ToString() => Nom;
}
