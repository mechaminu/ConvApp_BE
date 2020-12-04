using System;

namespace ConvAppServer.Models.Interfaces
{
    interface IModifiable
    {
        DateTime ModifiedDate { get; set; }
    }
}
