namespace Infrastructure
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public class ValidationErrors : Dictionary<string, List<string>>
    {
        public void AddErrorFor(string property, string error)
        {
            if (!this.ContainsKey(property))
            {
                this[property] = new List<string>();
            }

            this[property].Add(error);
        }

        public void AddErrorsFor(string property, Dictionary<string, List<string>> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                foreach (var error in kvp.Value)
                {
                    this.AddErrorFor(property, error);
                }
            }
        }

        public void AddErrorsFor(string property, ValidationErrors errors)
        {
            foreach (var kvp in this)
            {
                foreach (var error in kvp.Value)
                {
                    this.AddErrorFor(property, error);
                }
            }
        }

        private static readonly IReadOnlyCollection<string> EmptyCollection = new string[0];

        public IEnumerable<string> GetErrors(string propertyName)
        {
            return this.FirstOrDefault(it => it.Key == propertyName).Value.AsEnumerable() ?? EmptyCollection;
        }
    }

}