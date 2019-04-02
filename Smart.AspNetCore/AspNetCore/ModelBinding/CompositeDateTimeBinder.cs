namespace Smart.AspNetCore.ModelBinding
{
    using System;
    using System.Globalization;
    using System.Threading.Tasks;

    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public sealed class CompositeDateTimeBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var yearProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Year");
            var monthProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Month");
            var dayProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Day");
            var hourProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Hour");
            var minuteProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Minute");
            var secondProvider = bindingContext.ValueProvider.GetValue(bindingContext.ModelName + ".Second");

            if ((yearProvider == ValueProviderResult.None) &&
                (monthProvider == ValueProviderResult.None) &&
                (dayProvider == ValueProviderResult.None) &&
                (hourProvider == ValueProviderResult.None) &&
                (minuteProvider == ValueProviderResult.None) &&
                (secondProvider == ValueProviderResult.None))
            {
                return Task.CompletedTask;
            }

            var now = DateTimeOffset.Now;
            if (!ParseValue(bindingContext, yearProvider.FirstValue, now.Year, out int year) ||
                !ParseValue(bindingContext, monthProvider.FirstValue, now.Month, out int month) ||
                !ParseValue(bindingContext, dayProvider.FirstValue, now.Day, out int day) ||
                !ParseValue(bindingContext, hourProvider.FirstValue, 0, out int hour) ||
                !ParseValue(bindingContext, minuteProvider.FirstValue, 0, out int minute) ||
                !ParseValue(bindingContext, secondProvider.FirstValue, 0, out int second))
            {
                return Task.CompletedTask;
            }

            year = Math.Min(Math.Max(year, 1), 9999);
            month = Math.Min(Math.Max(month, 1), 12);
            day = Math.Min(Math.Max(day, 1), DateTime.DaysInMonth(year, month));
            hour = Math.Min(Math.Max(hour, 0), 23);
            minute = Math.Min(Math.Max(minute, 0), 59);
            second = Math.Min(Math.Max(second, 0), 59);

            object model = new DateTimeOffset(new DateTime(year, month, day, hour, minute, second));
            bindingContext.Result = ModelBindingResult.Success(model);

            return Task.CompletedTask;
        }

        private bool ParseValue(ModelBindingContext bindingContext, string str, int defaultValue, out int value)
        {
            if (String.IsNullOrEmpty(str))
            {
                value = defaultValue;
                return true;
            }

            try
            {
                value = Int32.Parse(str, NumberStyles.Integer, NumberFormatInfo.InvariantInfo);
                return true;
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException)
            {
                bindingContext.ModelState.AddModelError(
                    bindingContext.ModelName,
                    ex,
                    bindingContext.ModelMetadata);
                value = 0;
                return false;
            }
        }
    }
}
