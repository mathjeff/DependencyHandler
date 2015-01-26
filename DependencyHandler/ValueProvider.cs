﻿using System;
using System.Collections.Generic;
using System.Linq;
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

    interface ValueConverter<TInput, TOutput> : ValueReceiver<TInput>, ValueProvider<TOutput>
    {

    }
}
