using IPandDomainFilter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPandDomainFilter.Abstraction
{
    public interface IExceptionHandlingInDatabase
    {
        void StoreException(ErrorMessage errorMessage);

    }
}
