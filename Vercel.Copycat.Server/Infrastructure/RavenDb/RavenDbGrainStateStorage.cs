using Orleans.Runtime;
using Orleans.Storage;
using Raven.Client.Documents;

namespace Vercel.Copycat.Server.Infrastructure.RavenDb;

public class RavenDbGrainStateStorage(IDocumentStore store, ILogger<RavenDbGrainStateStorage> logger) : IGrainStorage
{
    public async Task ReadStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var docId = BuildGrainDocumentStateId(stateName, grainId);
        using var readSession = store.OpenAsyncSession();
        var state = await readSession.LoadAsync<T>(docId);
        grainState.State = state;
        grainState.RecordExists = state is not null;
        logger.LogInformation("load state for grain {DocId}", docId);
    }

    public async Task WriteStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var docId = BuildGrainDocumentStateId(stateName, grainId);
        using var writeSession = store.OpenAsyncSession();
        await writeSession.StoreAsync(grainState.State, docId);
        await writeSession.SaveChangesAsync();
        grainState.RecordExists = grainState.State is not null;
        logger.LogInformation("updated state for grain {DocId}", docId);
    }
    
    public async Task ClearStateAsync<T>(string stateName, GrainId grainId, IGrainState<T> grainState)
    {
        var docId = BuildGrainDocumentStateId(stateName, grainId);
        using var writeSession = store.OpenAsyncSession();
        writeSession.Delete(docId);
        await writeSession.SaveChangesAsync();
        logger.LogInformation("clear state for grain {DocId}", docId);
    }

    private static string BuildGrainDocumentStateId(string stateName, GrainId grainId) => grainId.Key.ToString() is not null
        ? $"{stateName}/{grainId.Key}"
        : string.Empty;
}