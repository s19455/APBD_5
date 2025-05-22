using TravelAgencyApi.DTOs;
using TravelAgencyApi.Models;

namespace TravelAgencyApi.Repositories;

public interface IClientRepository
{
    Task<IEnumerable<TripDto>> GetTripsAsync();
    Task<IEnumerable<TripDto>> GetTripsForClientAsync(int clientId);
    Task<int> CreateClientAsync(Client client);
    Task<bool> RegisterClientForTripAsync(int clientId, int tripId);
    Task<bool> DeleteClientTripAsync(int clientId, int tripId);
}