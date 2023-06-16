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

var preferences = new DataTable();
preferences.Columns.Add("UserId", typeof(string));
preferences.Columns.Add("Drama", typeof(List<int>));
preferences.Columns.Add("Comedy", typeof(List<int>));
preferences.Columns.Add("Action", typeof(List<int>)); // + Adventure
preferences.Columns.Add("Romance", typeof(List<int>));
preferences.Columns.Add("Fiction", typeof(List<int>)); // Science Fiction + Fantasy
preferences.Columns.Add("Animation", typeof(List<int>));
preferences.Columns.Add("Thriller", typeof(List<int>));
preferences.Columns.Add("Documentary", typeof(List<int>));
preferences.Columns.Add("UserObject", typeof(User)); 

using (var reader = new StreamReader("ratings_export.csv"))
using (var csv = new CsvReader(reader, csvConfig))
{ 
    csv.Read();
    csv.ReadHeader();
    preferences.PrimaryKey = new DataColumn[] {preferences.Columns["UserId"]}; // UserId у кожного індівідуальний
    while (csv.Read())
    {
        var rating = csv.GetRecord<RatingsExport>();
        if (movieIds.ContainsKey(rating.MovieID))
        {
            var row = preferences.Rows.Find(rating.UserID);
            if (row == null)
            {
                row = preferences.NewRow();
                row["UserId"] = rating.UserID;
                preferences.Rows.Add(row);
                // всього юзерів 7473
                var user = new User
                {
                    UserID = rating.UserID,
                    MoviesByGenres = new Dictionary<string, List<MovieRec>>()
                };
                row["UserObject"] = user;
            }
            var currentUser = (User)row["UserObject"];
            var movie = movieIds[rating.MovieID];
            var genres = movie.Genres.Split('"').Where(x => x != "[" && x != "]" && x != ",");
            foreach (var genre in genres)
            {
                if (preferences.Columns.Contains(genre))
                {
                    var m = new MovieRec(rating.MovieID, int.Parse(rating.Rating), movie.Popularity);
                    currentUser.Add(genre, m);
                    var existing = row.Field<List<int>?>(genre) ?? new List<int>(); // отримуємо існуюче або new List (якщо null)
                    existing.Add(int.Parse(rating.Rating));
                    row.SetField(genre, existing);
                }
            }
        }
    }
}

foreach (DataRow row in preferences.Rows)
{
    var current = (User)row["UserObject"];
    foreach (var group in current.MoviesByGenres)
    {
        var nameColumn = group.Key;
        // отут буде прописано нормалізація всіх оцінок і закидання їх в preferences
    }
}

Console.WriteLine('h');
