namespace Smart.AspNetCore;

using Microsoft.AspNetCore.Mvc.ModelBinding;

public static class ModelStateDictionaryExtensions
{
    public static bool IsValid(this ModelStateDictionary modelsState, string key)
    {
        return !modelsState.TryGetValue(key, out var entry) || (entry.Errors.Count == 0);
    }
}
