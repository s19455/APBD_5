namespace TravelAgencyApi.DTOs;

public class TripDto
{
    public int               IdTrip     { get; set; }
    public string            Name       { get; set; } = null!;
    public string?           Description{ get; set; }
    public DateTime          DateFrom   { get; set; }
    public DateTime          DateTo     { get; set; }
    public int               MaxPeople  { get; set; }
    public IEnumerable<string> Countries { get; set; } = Enumerable.Empty<string>();
}