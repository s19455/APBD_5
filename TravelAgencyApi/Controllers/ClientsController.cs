using Microsoft.AspNetCore.Mvc;
using TravelAgencyApi.DTOs;
using TravelAgencyApi.Models;
using TravelAgencyApi.Repositories;

namespace TravelAgencyApi.Controllers;

[ApiController]
[Route("api/clients")]
public class ClientsController : ControllerBase
{
    private readonly IClientRepository _repo;
    public ClientsController(IClientRepository repo) => _repo = repo;

    /// <summary>Zwrot wycieczek przypisanych do klienta.</summary>
    [HttpGet("{id:int}/trips")]
    public async Task<IActionResult> GetTripsForClient(int id)
    {
        var trips = await _repo.GetTripsForClientAsync(id);
        return trips.Any() ? Ok(trips) : NotFound($"Klient {id} nie istnieje lub nie ma wycieczek.");
    }

    /// <summary>Tworzy nowego klienta.</summary>
    [HttpPost]
    public async Task<IActionResult> CreateClient([FromBody] CreateClientDto dto)
    {
        var entity = new Client
        {
            FirstName = dto.FirstName,
            LastName  = dto.LastName,
            Email     = dto.Email,
            Telephone = dto.Telephone,
            Pesel     = dto.Pesel
        };

        int id = await _repo.CreateClientAsync(entity);
        return CreatedAtAction(nameof(GetTripsForClient), new { id }, new { id });
    }

    /// <summary>Rejestruje klienta na wycieczkę.</summary>
    [HttpPut("{id:int}/trips/{tripId:int}")]
    public async Task<IActionResult> Register(int id, int tripId)
    {
        bool ok = await _repo.RegisterClientForTripAsync(id, tripId);
        return ok ? NoContent() : Conflict("Brak miejsc lub rekord już istnieje.");
    }

    /// <summary>Usuwa rejestrację klienta z wycieczki.</summary>
    [HttpDelete("{id:int}/trips/{tripId:int}")]
    public async Task<IActionResult> Unregister(int id, int tripId)
    {
        bool ok = await _repo.DeleteClientTripAsync(id, tripId);
        return ok ? NoContent() : NotFound();
    }
}