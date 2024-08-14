using System.Text.Json;
using Orleans.Runtime;
using Orleans.Storage;
using StackExchange.Redis;

namespace Vercel.Copycat.Server.Infrastructure.Redis;

public class RedisGrainStateStorage(
    IDatabase redis, ILogger<RedisGrainStateStorage> logger) : IGrainStorage
{
    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var docId = BuildGrainDataStateId(stateName, grainId);
        var data = await redis.StringGetAsync(docId);
        if (data.IsNullOrEmpty)
        {
            grainState.State = default!;
            grainState.RecordExists = false;
            return;
        }
        
        grainState.RecordExists = true;
        grainState.State = JsonSerializer.Deserialize<T>(data!)!;
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var docId = BuildGrainDataStateId(stateName, grainId);
        await redis.StringSetAsync(docId, JsonSerializer.Serialize(grainState.State));
        grainState.RecordExists = grainState.State is not null;
        logger.LogInformation("updated state for grain {DocId}", docId);
    }

    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var docId = BuildGrainDataStateId(stateName, grainId);
        await redis.KeyDeleteAsync(docId);
        logger.LogInformation("clear state for grain {DocId}", docId);
    }
    
    private static string BuildGrainDataStateId(string stateName, GrainId grainId) => grainId.Key.ToString() is not null
        ? $"{stateName}/{grainId.Key}"
        : string.Empty;
}