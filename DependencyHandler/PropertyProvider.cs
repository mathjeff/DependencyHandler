using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/*namespace DependencyHandling
{
    interface PropertyProvider<TInput, TOutput>
    {
        TOutput GetValue(TInput item);
        void SetValue(TInput item, TOutput newValue);
    }

    class ConstantValue_Provider<TInput, TOutput> : PropertyProvider<TInput, TOutput>
    {
        public ConstantValue_Provider()
        {
            this.value = default(TOutput);
        }
        public ConstantValue_Provider(TOutput value)
        {
            this.value = value;
        }
        public TOutput GetValue(TInput item)
        {
            return this.value;
        }
        public void SetValue(TInput item, TOutput newValue)
        {
            this.value = newValue;
        }
        TOutput value;
    }
}
*/