namespace Smart.AspNetCore
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public static class ModelStateDictionaryExtensions
    {
        public static bool IsValid(this ModelStateDictionary mdelsState, string key)
        {
            return mdelsState[key]?.Errors.Count == 0;
        }
    }
}
