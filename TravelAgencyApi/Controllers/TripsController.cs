using Microsoft.AspNetCore.Mvc;
using TravelAgencyApi.Repositories;

namespace TravelAgencyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripsController : ControllerBase
{
    private readonly IClientRepository _repo;
    public TripsController(IClientRepository repo) => _repo = repo;

    /// <summary>Pobiera wszystkie dostępne wycieczki.</summary>
    [HttpGet]
    public async Task<IActionResult> GetTrips()
        => Ok(await _repo.GetTripsAsync());
}