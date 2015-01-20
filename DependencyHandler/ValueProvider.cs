using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    interface ValueProvider<T>
    {
        T GetValue();
        void SetValue(T newValue);
    }

    class ConstantValue_Provider<T> : ValueProvider<T>
    {
        public ConstantValue_Provider()
        {
            this.value = default(T);
        }
        public ConstantValue_Provider(T value)
        {
            this.value = value;
        }
        public T GetValue()
        {
            return this.value;
        }
        public void SetValue(T newValue)
        {
            this.value = newValue;
        }
        T value;
    }
}
