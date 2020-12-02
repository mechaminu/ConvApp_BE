using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models.Interfaces
{
    interface IRankable
    {
        double MonthlyScore { get; set; }
        double SeasonalScore { get; set; }
        double AlltimeScore { get; set; }
    }
}
