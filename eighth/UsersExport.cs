using CsvHelper.Configuration.Attributes;

namespace eighth;

public class UsersExport
{
    //_id,display_name,num_ratings_pages,num_reviews,username
    [Name("_id")] public string Id { get; set; }
    [Name("display_name")] public string? Name { get; set; }
    [Name("num_ratings_pages")] public string? NumRatPages { get; set; }
    [Name("num_reviews")] public string? NumReviews { get; set; }
    [Name("username")] public string? Username { get; set; }
}