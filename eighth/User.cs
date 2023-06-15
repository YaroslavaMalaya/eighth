namespace eighth;

public class User
{
    public string UserID { get; set; }
    public Dictionary<string, List<string>> MoviesByGenres; // це щоб ми знаходили за жанром фільми які можемо порекомендувати

    public void Add(string genre, string movie)
    {
        if (MoviesByGenres.ContainsKey(genre))
        {
            var movies = MoviesByGenres[genre];
            movies.Add(movie);
        }
        else
        {
            MoviesByGenres.Add(genre, new List<string>{movie});
        }
    }
}