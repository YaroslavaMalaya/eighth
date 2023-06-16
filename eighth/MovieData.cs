using CsvHelper.Configuration.Attributes;

namespace eighth;

public class MovieData
{
    //[Name("_id")] public string Id { get; set; }
    [Name("genres")] public string? Genres { get; set; }
    //[Name("image_url")] public string? URL { get; set; }
    //[Name("imdb_id")] public string? ImdbId { get; set; }
    //[Name("imdb_link")] public string? ImdbLink { get; set; }
    [Name("movie_id")] public string? MovieId { get; set; }
    //[Name("movie_title")] public string? MovieTitle { get; set; }
    //[Name("original_language")] public string? LanguageOr { get; set; }
    //[Name("overview")] public string? Overview { get; set; }
    [Name("popularity")] public string? Popularity { get; set; }
    /*[Name("production_countries")] public string? Country { get; set; }
    [Name("release_date")] public string? Date { get; set; }
    [Name("runtime")] public string? Runtime { get; set; }
    [Name("spoken_languages")] public string? LanguagesSp { get; set; }
    [Name("tmdb_id")] public string? TmdbId { get; set; }
    [Name("tmdb_link")] public string? TmdbLink { get; set; }
    [Name("vote_average")]public string? VoteAverage { get; set; }
    [Name("vote_count")] public string? VoteCount { get; set; }
    [Name("year_released")] public string? YearReleased { get; set; }*/
}

public class MoviePop
{
    public string Genres;
    public double Popularity;

    public MoviePop(string g, double p)
    {
        Genres = g;
        Popularity = p;
    }
}