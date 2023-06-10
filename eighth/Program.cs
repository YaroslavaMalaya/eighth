using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;

var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    MissingFieldFound = null
};

using (var reader = new StreamReader("movie_data.csv"))
using (var csv = new CsvReader(reader, csvConfig))
{
    var movies = csv.GetRecords<MovieData>();
    var movies_genres = movies.Where(movie =>
        movie.Genres != null &&
        (movie.Genres.Contains("Drama") ||
        movie.Genres.Contains("Comedy") ||
        movie.Genres.Contains("Action") ||
        movie.Genres.Contains("Adventure") ||
        movie.Genres.Contains("Romance") ||
        movie.Genres.Contains("Science Fiction") ||
        movie.Genres.Contains("Fantasy") ||
        movie.Genres.Contains("Animation") ||
        movie.Genres.Contains("Thriller") ||
        movie.Genres.Contains("Documentary"))
    );
    Console.WriteLine(movies_genres.Count());
}

Console.WriteLine('h');

public class MovieData
{
    [Name("_id")]
    public string Id { get; set; }
    [Name("genres")]
    public string? Genres { get; set; }
    [Name("image_url")]
    public string? URL { get; set; }
    public string? imdb_id { get; set; }
    public string? imdb_link { get; set; }
    public string? movie_id { get; set; }
    public string? movie_title { get; set; }
    public string? original_language { get; set; }
    public string? overview { get; set; }
    public string? popularity { get; set; }
    public string? production_countries { get; set; }
    public string? release_date { get; set; }
    public string? runtime { get; set; }
    public string? spoken_languages { get; set; }
    public string? tmdb_id { get; set; }
    public string? tmdb_link { get; set; }
    public string? vote_average { get; set; }
    public string? vote_count { get; set; }
    public string? year_released { get; set; }
}
