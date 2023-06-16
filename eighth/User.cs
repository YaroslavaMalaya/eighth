namespace eighth;

public class MovieRec
{
    public string MovieId { get; set; }
    public int Rating { get; set; }
    public double Popularity { get; set; }

    public MovieRec(string movieId, int rating, double popularity)
    {
        MovieId = movieId;
        Rating = rating;
        Popularity = popularity;
    }
}

public class User
{
    public string UserID { get; set; }
    public Dictionary<string, List<MovieRec>> MoviesByGenres; // це щоб ми знаходили за жанром фільми які можемо порекомендувати

    public void Add(string genre, MovieRec movie)
    {
        if (MoviesByGenres.ContainsKey(genre))
        {
            MoviesByGenres[genre].Add(movie);
        }
        else
        {
            MoviesByGenres.Add(genre, new List<MovieRec>{movie});
        }
    }
}