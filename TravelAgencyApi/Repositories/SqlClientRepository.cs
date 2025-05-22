using Microsoft.Data.SqlClient;
using System.Data;
using TravelAgencyApi.DTOs;
using TravelAgencyApi.Models;

namespace TravelAgencyApi.Repositories;

public class SqlClientRepository : IClientRepository
{
    private readonly string _connStr;
    public SqlClientRepository(IConfiguration cfg)
        => _connStr = cfg.GetConnectionString("Default")!;

    private SqlConnection Conn() => new(_connStr);

    // ---------- GET /api/trips ----------
    public async Task<IEnumerable<TripDto>> GetTripsAsync()
    {
        const string sql = """
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               c.Name AS CountryName
          FROM Trip t
          JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
          JOIN Country c       ON ct.IdCountry = c.IdCountry
        """;

        var dict = new Dictionary<int, TripDto>();

        await using var con = Conn();
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        await using var rdr = await cmd.ExecuteReaderAsync();

        while (await rdr.ReadAsync())
        {
            int id = rdr.GetInt32(0);
            if (!dict.TryGetValue(id, out var dto))
            {
                dto = new()
                {
                    IdTrip      = id,
                    Name        = rdr.GetString(1),
                    Description = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    DateFrom    = rdr.GetDateTime(3),
                    DateTo      = rdr.GetDateTime(4),
                    MaxPeople   = rdr.GetInt32(5),
                    Countries   = new List<string>()
                };
                dict.Add(id, dto);
            }
            ((List<string>)dto.Countries).Add(rdr.GetString(6));
        }
        return dict.Values;
    }

    // ---------- GET /api/clients/{id}/trips ----------
    public async Task<IEnumerable<TripDto>> GetTripsForClientAsync(int clientId)
    {
        const string sql = """
        SELECT t.IdTrip, t.Name, t.Description, t.DateFrom, t.DateTo, t.MaxPeople,
               c.Name AS CountryName, ct.RegisteredAt, ct.PaymentDate
          FROM Trip t
          JOIN Client_Trip ct2 ON t.IdTrip = ct2.IdTrip
          JOIN Client c2       ON c2.IdClient = ct2.IdClient
          JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip
          JOIN Country c       ON c.IdCountry = ct.IdCountry
         WHERE c2.IdClient = @id
        """;

        var dict = new Dictionary<int, TripDto>();

        await using var con = Conn();
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@id", clientId);

        await using var rdr = await cmd.ExecuteReaderAsync();

        while (await rdr.ReadAsync())
        {
            int id = rdr.GetInt32(0);
            if (!dict.TryGetValue(id, out var dto))
            {
                dto = new()
                {
                    IdTrip      = id,
                    Name        = rdr.GetString(1),
                    Description = rdr.IsDBNull(2) ? null : rdr.GetString(2),
                    DateFrom    = rdr.GetDateTime(3),
                    DateTo      = rdr.GetDateTime(4),
                    MaxPeople   = rdr.GetInt32(5),
                    Countries   = new List<string>()
                };
                dict.Add(id, dto);
            }
            ((List<string>)dto.Countries).Add(rdr.GetString(6));
        }
        return dict.Values;
    }

    // ---------- POST /api/clients ----------
    public async Task<int> CreateClientAsync(Client client)
    {
        const string sql = """
        INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel)
        OUTPUT INSERTED.IdClient
        VALUES (@fn, @ln, @em, @tel, @pesel)
        """;

        await using var con = Conn();
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);

        cmd.Parameters.AddWithValue("@fn",   client.FirstName);
        cmd.Parameters.AddWithValue("@ln",   client.LastName);
        cmd.Parameters.AddWithValue("@em",   client.Email);
        cmd.Parameters.AddWithValue("@tel",  client.Telephone);
        cmd.Parameters.AddWithValue("@pesel",client.Pesel);

        return (int)await cmd.ExecuteScalarAsync();
    }

    // ---------- PUT /api/clients/{id}/trips/{tripId} ----------
    public async Task<bool> RegisterClientForTripAsync(int clientId, int tripId)
    {
        await using var con = Conn();
        await con.OpenAsync();
        await using var tx = con.BeginTransaction(IsolationLevel.Serializable);

        // 1. czy klient istnieje
        var existsCmd = new SqlCommand("SELECT 1 FROM Client WHERE IdClient=@c", con, tx);
        existsCmd.Parameters.AddWithValue("@c", clientId);
        if (await existsCmd.ExecuteScalarAsync() is null)
        { await tx.RollbackAsync(); return false; }

        // 2. wolne miejsca?
        var seatCmd = new SqlCommand("""
            SELECT MaxPeople - COUNT(*) 
              FROM Trip t LEFT JOIN Client_Trip ct ON t.IdTrip = ct.IdTrip
             WHERE t.IdTrip = @t
             GROUP BY t.MaxPeople
        """, con, tx);
        seatCmd.Parameters.AddWithValue("@t", tripId);
        var seatsObj = await seatCmd.ExecuteScalarAsync();
        if (seatsObj is null || (int)seatsObj <= 0)
        { await tx.RollbackAsync(); return false; }

        // 3. insert
        var ins = new SqlCommand("""
            INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
            VALUES (@c,@t,SYSDATETIME())
        """, con, tx);
        ins.Parameters.AddWithValue("@c", clientId);
        ins.Parameters.AddWithValue("@t", tripId);
        await ins.ExecuteNonQueryAsync();

        await tx.CommitAsync();
        return true;
    }

    // ---------- DELETE /api/clients/{id}/trips/{tripId} ----------
    public async Task<bool> DeleteClientTripAsync(int clientId, int tripId)
    {
        const string sql = "DELETE FROM Client_Trip WHERE IdClient=@c AND IdTrip=@t";
        await using var con = Conn();
        await con.OpenAsync();
        await using var cmd = new SqlCommand(sql, con);
        cmd.Parameters.AddWithValue("@c", clientId);
        cmd.Parameters.AddWithValue("@t", tripId);
        return await cmd.ExecuteNonQueryAsync() > 0;
    }
}
