using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    class StringJoiner : ValueProvider<string>
    {
        public StringJoiner(IEnumerable<ValueProvider<string>> providers)
        {
            this.initialize(providers);
        }
        public StringJoiner(ValueProvider<string> first, ValueProvider<string> second)
        {
            LinkedList<ValueProvider<string>> providers = new LinkedList<ValueProvider<string>>();
            providers.AddLast(first);
            providers.AddLast(second);
            this.initialize(providers);
        }
        private void initialize(IEnumerable<ValueProvider<string>> providers)
        {
            this.providers = providers;
        }
        public string GetValue()
        {
            string result = "";
            foreach (ValueProvider<string> provider in this.providers)
            {
                result += provider.GetValue();
            }
            return result;
        }
        public void SetValue(string newValue)
        {
            throw new InvalidOperationException();
        }
        private IEnumerable<ValueProvider<string>> providers;
    }
}
