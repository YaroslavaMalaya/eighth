using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using eighth;
    
Dictionary<string, MoviePop> movieIds;
var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
{
    NewLine = Environment.NewLine
};

using (var reader = new StreamReader("movie_data.csv"))
using (var csv = new CsvReader(reader, csvConfig))
{
    var movies = csv.GetRecords<MovieData>();
    var movies_genres = movies.Where(movie =>
        movie.Genres != null && movie.MovieId != null &&
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
    ).ToList();
    
    movieIds = movies_genres.ToDictionary(x => x.MovieId, x => new MoviePop(x.Genres, double.Parse(x.Popularity.Replace('.', ','))));
}

var users = new Dictionary<string, User>();
var genres = new List<string> { "Drama", "Comedy", "Action", "Romance", "Fiction", "Animation", "Thriller", "Documentary" };
using (var reader = new StreamReader("ratings_export.csv"))
using (var csv = new CsvReader(reader, csvConfig))
{ 
    csv.Read();
    csv.ReadHeader();
    while (csv.Read())
    {
        var rating = csv.GetRecord<RatingsExport>();
        if (movieIds.ContainsKey(rating.MovieID))
        {
            if (!users.ContainsKey(rating.UserID))
            {
                // всього юзерів 7473
                var user = new User
                {
                    UserID = rating.UserID,
                    MoviesByGenres = new Dictionary<string, List<MovieRec>>()
                };
                users.Add(rating.UserID, user);
            }
            var currentUser = users[rating.UserID];
            var movie = movieIds[rating.MovieID];
            var movieGenres = movie.Genres.Split('"').Where(x => x != "[" && x != "]" && x != ",");
            foreach (var genre in movieGenres)
            {
                if (genres.Contains(genre))
                {
                    var m = new MovieRec(rating.MovieID, int.Parse(rating.Rating), movie.Popularity);
                    currentUser.Add(genre, m);
                }
            }
        }
    }
}

var preferences = new DataTable();
preferences.Columns.Add("UserId", typeof(string));
preferences.Columns.Add("Drama", typeof(double));
preferences.Columns.Add("Comedy", typeof(double));
preferences.Columns.Add("Action", typeof(double)); // + Adventure
preferences.Columns.Add("Romance", typeof(double));
preferences.Columns.Add("Fiction", typeof(double)); // Science Fiction + Fantasy
preferences.Columns.Add("Animation", typeof(double));
preferences.Columns.Add("Thriller", typeof(double));
preferences.Columns.Add("Documentary", typeof(double));
preferences.PrimaryKey = new DataColumn[] { preferences.Columns["UserId"] };

foreach (var userPair in users)
{
    var row = preferences.Rows.Find(userPair.Key);
    if (row == null)
    {
        row = preferences.NewRow();
        row["UserId"] = userPair.Key;
        preferences.Rows.Add(row);
    }

    foreach (var (genre, movies) in userPair.Value.MoviesByGenres)
    {
        var ratings = movies.Select(movie => movie.Rating).ToList();
        if (ratings.Count == 0)
        {
            row.SetField(genre, 0.0);
        }
        else if (ratings.Count >= 4)
        {
            row.SetField(genre, Math.Round(Normalize(ratings), 1));
        }
        else
        {
            row.SetField(genre, Math.Round(NormalizeForSmallAmount(ratings), 1));
        }
    }
    
    foreach (DataColumn column in preferences.Columns)
    {
        if (row.IsNull(column))
        {
            row[column] = 0.0;
        }
    }
}


// це можна подивитися по кожному користувачу його вподобання
/*foreach (DataRow row in preferences.Rows) 
{
    Console.WriteLine($"User: {row["UserId"]}");
    Console.WriteLine($"Drama: {row["Drama"]}");
    Console.WriteLine($"Comedy: {row["Comedy"]}");
    Console.WriteLine($"Action: {row["Action"]}");
    Console.WriteLine($"Romance: {row["Romance"]}");
    Console.WriteLine($"Fiction: {row["Fiction"]}");
    Console.WriteLine($"Animation: {row["Animation"]}");
    Console.WriteLine($"Thriller: {row["Thriller"]}");
    Console.WriteLine($"Documentary: {row["Documentary"]}");
    Console.WriteLine();
}*/

Console.WriteLine('h');

double Normalize(List<int> ratings)
{
    var minRating = ratings.Min();
    var maxRating = ratings.Max();
    if (minRating == maxRating)
    {
        return minRating / 10;
    }
    var normalize = new List<double>();
    foreach (var element in ratings)
    {
        double norm = (element - minRating) / (maxRating - minRating);
        normalize.Add(norm);
    }
    return normalize.Average();
}

double NormalizeForSmallAmount(List<int> ratings)
{
    var sum = ratings.Average();
    return sum / 10;
}