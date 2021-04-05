namespace Smart.AspNetCore.ModelBinding
{
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public static class ModelStateDictionaryExtensions
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:ValidateArgumentsOfPublicMethods", Justification = "Extensions")]
        public static bool IsValid(this ModelStateDictionary modelsState, string key)
        {
            return modelsState[key]?.Errors.Count == 0;
        }
    }
}
