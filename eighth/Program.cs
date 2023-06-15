using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using eighth;
    
List<MovieData> movies_genres;
Dictionary<string?, string?> movieIds;
var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    NewLine = Environment.NewLine
};

using (var reader = new StreamReader("movie_data.csv"))
using (var csv = new CsvReader(reader, csvConfig))
{
    var movies = csv.GetRecords<MovieData>();
    movies_genres = movies.Where(movie =>
        movie.Genres.Contains("Drama") ||
        movie.Genres.Contains("Comedy") ||
        movie.Genres.Contains("Action") ||
        movie.Genres.Contains("Adventure") ||
        movie.Genres.Contains("Romance") ||
        movie.Genres.Contains("Science Fiction") ||
        movie.Genres.Contains("Fantasy") ||
        movie.Genres.Contains("Animation") ||
        movie.Genres.Contains("Thriller") ||
        movie.Genres.Contains("Documentary")
    ).ToList();
    
    movieIds = movies_genres.ToDictionary(x => x.MovieId, x => x.Genres);
    //movieIds = new Dictionary<string, string>(movies_genres.Select(movie => (movie.MovieId, movie.Genres)));
}

var preferencesTable = new DataTable();
preferencesTable.Columns.Add("UserId", typeof(string));
preferencesTable.Columns.Add("Drama", typeof(int));
preferencesTable.Columns.Add("Comedy", typeof(int));
preferencesTable.Columns.Add("Action", typeof(int)); // + Adventure
preferencesTable.Columns.Add("Romance", typeof(int));
preferencesTable.Columns.Add("Fiction", typeof(int)); // Science Fiction + Fantasy
preferencesTable.Columns.Add("Animation", typeof(int));
preferencesTable.Columns.Add("Thriller", typeof(int));
preferencesTable.Columns.Add("Documentary", typeof(int));

var user = new User();
using (var reader = new StreamReader("ratings_export.csv"))
using (var csv = new CsvReader(reader, csvConfig))
{ 
    csv.Read();
    csv.ReadHeader();
    preferencesTable.PrimaryKey = new DataColumn[] {preferencesTable.Columns["UserId"]};
    while (csv.Read())
    {
        var rating = csv.GetRecord<RatingsExport>();
        if (movieIds.ContainsKey(rating.MovieID))
        {
            var row = preferencesTable.Rows.Find(rating.UserID);
            if (row == null)
            {
                row = preferencesTable.NewRow();
                row["UserId"] = rating.UserID;
                preferencesTable.Rows.Add(row);
                // всього юзерів 7473
                user.UserID = rating.UserID;
                user.MoviesByGenres = new Dictionary<string, List<string>>(); 
            }
            var movie = movieIds[rating.MovieID];
            var genres = movie.Split('"').Where(x => x != "[" && x != "]" && x != ",");
            foreach (var genre in genres)
            {
                if (preferencesTable.Columns.Contains(genre))
                {
                    user.Add(genre, rating.MovieID);
                    //  ТРЕБА НОРМАЛІЗУВАТИ
                    var existing = row.Field<int?>(genre) ?? 0; // отримуємо існуюче значення або 0 (якщо null)
                    row.SetField(genre, (existing + int.Parse(rating.Rating)));
                }
            }
        }
    }
}

Console.WriteLine('h');
