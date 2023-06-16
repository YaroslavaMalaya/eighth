using CsvHelper.Configuration.Attributes;

namespace eighth;

public class RatingsExport 
{
    //_id,movie_id,rating_val,user_id
    //[Name("_id")] public string Id { get; set; }
    [Name("movie_id")] public string MovieID { get; set; }
    [Name("rating_val")] public string Rating { get; set; }
    [Name("user_id")] public string UserID { get; set; }
}