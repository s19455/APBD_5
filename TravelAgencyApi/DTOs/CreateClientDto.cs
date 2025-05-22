namespace TravelAgencyApi.DTOs;

public class CreateClientDto
{
    public string FirstName { get; set; } = null!;
    public string LastName  { get; set; } = null!;
    public string Email     { get; set; } = null!;
    public string Telephone { get; set; } = null!;
    public string Pesel     { get; set; } = null!;
}