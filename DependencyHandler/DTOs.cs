using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyHandling
{
    interface DTO<T> : ValueProvider<T>
    {

    }
}
