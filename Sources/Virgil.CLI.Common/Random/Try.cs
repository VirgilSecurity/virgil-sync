namespace Virgil.CLI.Common.Random
{
    using System.Collections.Generic;
    using System.Linq;

    public class Try
    {
        protected readonly List<string> Errors = new List<string>();

        public void AddError(string error)
        {
            this.Errors.Add(error);
        }

        public bool IsValid()
        {
            return !this.Errors.Any();
        }

        public List<string> GetErrors()
        {
            return this.Errors;
        }
    }

    public class Try<T> : Try
    {
        public void SetResult(T result)
        {
            this.Result = result;
        }

        public T Result { get; private set; } = default(T);
    }
}