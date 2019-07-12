namespace Smart.AspNetCore.ModelBinding
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public static class ModelStateDictionaryExtensions
    {
        public static bool IsValid(this ModelStateDictionary modelsState, string key)
        {
            return modelsState[key]?.Errors.Count == 0;
        }
    }
}
