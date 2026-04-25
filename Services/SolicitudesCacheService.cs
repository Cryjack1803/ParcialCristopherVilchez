using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ParcialVilchezCristopher_.Models;

namespace ParcialVilchezCristopher_.Services;

public class SolicitudesCacheService
{
    private static readonly DistributedCacheEntryOptions CacheOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
    };

    private readonly IDistributedCache _cache;

    public SolicitudesCacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<List<SolicitudCredito>?> GetSolicitudesAsync(string usuarioId)
    {
        var cached = await _cache.GetStringAsync(GetKey(usuarioId));
        if (string.IsNullOrWhiteSpace(cached))
        {
            return null;
        }

        return JsonSerializer.Deserialize<List<SolicitudCredito>>(cached);
    }

    public Task SetSolicitudesAsync(string usuarioId, List<SolicitudCredito> solicitudes)
    {
        var payload = JsonSerializer.Serialize(solicitudes);
        return _cache.SetStringAsync(GetKey(usuarioId), payload, CacheOptions);
    }

    public Task InvalidateSolicitudesAsync(string usuarioId)
    {
        return _cache.RemoveAsync(GetKey(usuarioId));
    }

    private static string GetKey(string usuarioId)
    {
        return $"solicitudes:{usuarioId}";
    }
}