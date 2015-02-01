using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    interface ValueProvider<TOutput>
    {
        TOutput GetValue();
        void SetValue(TOutput newValue);
    }

    class ConstantValue_Provider<TOutput> : ValueProvider<TOutput>
    {
        public ConstantValue_Provider()
        {
            this.value = default(TOutput);
        }
        public ConstantValue_Provider(TOutput value)
        {
            this.value = value;
        }
        public TOutput GetValue()
        {
            return this.value;
        }
        public void SetValue(TOutput newValue)
        {
            this.value = newValue;
        }
        TOutput value;
    }

    interface ValueReceiver<TInput>
    {
        void SetInput(TInput input);
    }

    interface ValueConverter<TInput, TOutput>
    {
        TOutput ConvertValue(TInput input);
    }

    class ConstantValue_Converter<TInput, TOutput> : ValueProvider<TOutput>
    {
        public ConstantValue_Converter(TInput input, ValueConverter<TInput, TOutput> converter)
        {
            this.input = input;
            this.converter = converter;
        }
        public TOutput GetValue()
        {
            return this.converter.ConvertValue(this.input);
        }
        public void SetValue(TOutput ouptut)
        {
            throw new InvalidOperationException();
        }
        private TInput input;
        private ValueConverter<TInput, TOutput> converter;
    }

    class NewInstance_Provider : ValueConverter<Type, object>
    {
        public NewInstance_Provider()
        {
        }
        public NewInstance_Provider(Type type)
        {
            this.outputType = type;
        }
        public NewInstance_Provider(object example)
        {
            this.outputType = example.GetType();
        }
        public object ConvertValue(Type defaultType)
        {
            Type type;
            if (this.outputType != null)
                type = this.outputType;
            else
                type = defaultType;

            ConstructorInfo constructor = type.GetConstructor(new Type[0]);
            if (constructor == null)
            {
                throw new InvalidOperationException("No default constructor found for " + type);

            }
            else
            {
                object item = constructor.Invoke(new object[0]);
                return item;
            }
        }

        Type outputType;

    }
}
