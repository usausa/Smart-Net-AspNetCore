namespace Smart.AspNetCore.Routing
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public abstract class ConvertAttribute : Attribute
    {
        public abstract object Convert(object source);
    }
}
