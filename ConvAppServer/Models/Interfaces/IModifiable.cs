using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models.Interfaces
{
    interface IModifiable
    {
        DateTime ModifiedDate { get; set; }
    }
}
