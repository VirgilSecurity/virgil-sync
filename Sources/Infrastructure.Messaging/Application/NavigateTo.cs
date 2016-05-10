namespace Infrastructure.Messaging.Application
{
    using System;

    public class NavigateTo
    {
        public NavigateTo(Type type)
        {
            this.Type = type;
        }

        public NavigateTo(Type type, object @params)
        {
            this.Type = type;
            this.Params = @params;
        }

        public Type Type { get; }
        public object Params { get; }
    }
}