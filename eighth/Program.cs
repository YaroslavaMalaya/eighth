﻿using System.Data;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using eighth;


Dictionary<string, MoviePop> movieIds;
Dictionary<string, string?> check_correct;
var genres = new List<string> { "Drama", "Comedy", "Action", "Romance", "Fiction", "Animation", "Thriller", "Documentary" };

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

    check_correct = movies_genres.ToDictionary(x => x.MovieId, x => x.MovieTitle);
    movieIds = movies_genres.ToDictionary(x => x.MovieId, x => 
        new MoviePop(x.MovieId, x.Genres, double.Parse(x.Popularity.Replace('.', ','))));
}

var users = new Dictionary<string, User>();
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
            RatingGathering(movieId: rating.MovieID, userId: rating.UserID, rating: rating.Rating);
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
preferences.Columns.Add("UserObject", typeof(User));
preferences.PrimaryKey = new DataColumn[] { preferences.Columns["UserId"] };

foreach (var userPair in users)
{
    RatingProcessing(userPair);
}

Console.WriteLine('h');

var count = 0; // для підрахунку скільки людина вже оцінила фільмів

while (true)
{
    Console.WriteLine("\nYou can rate the film here (please enter the 'rate' first):");
    var input = Console.ReadLine().Split(" ");
    var movie_rate = string.Join(" ", input.Skip(1).Take(input.Length - 2).ToList());
    var rating_rate = input[^1];

    if (!check_correct.ContainsValue(movie_rate))
    {
        var distances = new Dictionary<string, int>();
        foreach (var word in check_correct)
        {
            distances[word.Key] = DamerauLevenshteinDistance(movie_rate, word.Key);
        }

        var sortedKeyValuePairs = distances.OrderBy(x => x.Value).Take(1).ToList();
        var suggestions = sortedKeyValuePairs.Select(pair => pair.Key).ToList();

        Console.WriteLine($"\nNo such film found :( Closest candidates: {string.Join(", ", suggestions)}");
    }
    else
    {
        var movie_id = check_correct.First(x => x.Value == movie_rate).Key;
        RatingGathering(movieId: movie_id, userId: "NewUser", rating: rating_rate);
        count++;
        Console.WriteLine($"\nYou've rated a film '{movie_rate}' ({movie_id}) as {double.Parse(rating_rate)}");
    }

    Console.WriteLine("\nDo you want to rate more movies or take a look at recommendations? (enter 'rate', 'recommend', or 'discovery)'");
    var check = Console.ReadLine();
    switch (check)
    {
        case "rate":
            continue;
        case "recommend":
            RatingProcessing(users.Last()); // Add the new user with their rated movies to the table
            var recommendations = GetMovieRecommendations(users, movieIds, check_correct);
            Console.WriteLine("Here are your recommendations:");
            for (var i = 0; i < recommendations.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {recommendations[i]}");
            }
            break;
        case "discovery":
            DiscoveryMode(users, movieIds, check_correct);
            break;
        default:
            Console.WriteLine("It looks like you are tired. Go to bed.");
            break;
    }
    break;
}

void RatingGathering(string userId, string movieId, string rating)
{
    if (!users.ContainsKey(userId))
    {
        // всього юзерів 7473
        var user = new User
        {
            UserID = userId,
            MoviesByGenres = new Dictionary<string, List<MovieRec>>()
        };
        users.Add(userId, user);
    }   
    var current_user = users[userId];
    var movie = movieIds[movieId];
    var movie_genres = movie.Genres.Split('"').Where(x => x != "[" && x != "]" && x != ",");
    foreach (var genre in movie_genres)
    {
        if (genres.Contains(genre))
        {
            var m = new MovieRec(movieId, int.Parse(rating), movie.Popularity);
            current_user.Add(genre, m);
        }
    }
}


void RatingProcessing(KeyValuePair<string, User> user)
{
    var row = preferences.Rows.Find(user.Key);
    if (row == null)
    {
        row = preferences.NewRow();
        row["UserId"] = user.Key;
        preferences.Rows.Add(row);
        row["UserObject"] = user.Value;
    }
    
    foreach (var (genre, movies) in user.Value.MoviesByGenres)
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

double Normalize(List<int> ratings)
{
    var min = ratings.Min();
    var max = ratings.Max();
    if (min == max)
    {
        return min / 10;
    }
    var normalize = new List<double>();
    foreach (var element in ratings)
    {
        double norm = (element - min) / (max - min);
        normalize.Add(norm);
    }
    return normalize.Average();
}

double NormalizeForSmallAmount(List<int> ratings)
{
    var sum = ratings.Average();
    return sum / 10;
}

void DiscoveryMode(Dictionary<string, User> users, Dictionary<string, MoviePop> movieIds, Dictionary<string, string?> check_correct)
{
    Console.WriteLine("Welcome to the discovery!");

    var movieRecommendations = GetMovieRecommendations(users, movieIds, check_correct);
    var user = users.Last().Value;
    var answeredGenres = new List<string>();
    


    foreach (var genre in genres)
    {
        if (answeredGenres.Contains(genre))
            continue;

        Console.WriteLine($"Do you like {genre} movies? (Yes/No)");
        var response = Console.ReadLine();

        if (response.Equals("Yes", StringComparison.OrdinalIgnoreCase))
        {
            var moviesInGenre = movieIds.Values.Where(movie => movie.Genres.Contains(genre)).ToList();
            var unratedMovies = moviesInGenre
                .Where(movie => !user.MoviesByGenres.ContainsKey(movie.MovieId))
                .OrderByDescending(movie => movie.Popularity)
                .Take(2) // Limit the number of movies in each genre
                .ToList();

           
            foreach (var movie in unratedMovies)
            {
                Console.WriteLine($"Have you seen '{movieIds[movie.MovieId].MovieId}'? (Yes/No)");

                var movieResponse = Console.ReadLine();

                if (movieResponse.Equals("Yes", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("How would you rate it?");
                    var rating = Console.ReadLine();
                    RatingGathering(userId: "NewUser", movieId: movie.MovieId, rating: rating);
                }
            }

            answeredGenres.Add(genre);
        }
    }
   
    Console.WriteLine("Thank you for your responses! Generating recommendations...");
    RatingProcessing(users.Last()); // Add the new user with their rated movies to the table

    // Display recommendations
    movieRecommendations = GetMovieRecommendations(users, movieIds, check_correct);
    Console.WriteLine("Here are your recommendations:");
    for (var i = 0; i < movieRecommendations.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {movieRecommendations[i]}");
    }
}


List<string> GetMovieRecommendations(Dictionary<string, User> users, Dictionary<string, MoviePop> movieIds, Dictionary<string, string?> check_correct)
{
    var user = users.Last().Value;
    var movieRecommendations = new List<string>();
    var count = 0;

    foreach (var movieId in movieIds.Keys)
    {
        if (count >= 10) // Limit the number of recommendations to 10
            break;

        var movie = movieIds[movieId];

        // Check if the user has watched the movie
        if (!user.MoviesByGenres.ContainsKey(movieId))
        {
            var relevantGenres = movie.Genres.Split(", ");
            var hasRelevantGenre = false;

            foreach (var genre in relevantGenres)
            {
                if (user.MoviesByGenres.ContainsKey(genre))
                {
                    hasRelevantGenre = true;
                    break;
                }
            }

            if (hasRelevantGenre)
            {
                var averageRatings = relevantGenres
                    .Where(genre => user.MoviesByGenres.ContainsKey(genre))
                    .Select(genre => user.MoviesByGenres[genre].Select(movieRec => movieRec.Rating).Average());

                var averageRating = averageRatings.Any() ? averageRatings.Average() : 0;

                if (averageRating >= 7.0)
                {
                    movieRecommendations.Add(check_correct[movieId]);
                    count++;
                }
            }
        }
    }

    return movieRecommendations;
}



int DamerauLevenshteinDistance(string word1, string word2)
{
    var w1 = word1.Length;
    var w2 = word2.Length;
    

    var matrix = new int[w1 + 1, w2 + 1];
    for (var j = 0; j <= w2; j++)
        matrix[0, j] = j; // прописує рядок з індексами
    for (var i = 0; i <= w1; i++)
        matrix[i, 0] = i; // прописує стовбець з індексами

    for (var j = 1; j <= w2; j++)
    {
        for (var i = 1; i <= w1; i++)
        {
            var cost = word1[i - 1] == word2[j - 1] ? 0 : 1;
            matrix[i, j] = Math.Min(
                Math.Min(matrix[i - 1, j] + 1, matrix[i, j - 1] + 1),
                matrix[i - 1, j - 1] + cost);
            
            if (i > 1 && j > 1 && word1[i - 1] == word2[j - 2] && word1[i - 2] == word2[j - 1])
            {
                matrix[i, j] = Math.Min(matrix[i, j], matrix[i - 2, j - 2] + cost);
            }
        }
    }
    return matrix[w1 , w2];
}