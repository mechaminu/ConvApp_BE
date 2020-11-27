using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvAppServer.Models
{
    public class View
    {
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public byte Type { get; set; }
        public int Id { get; set; }
    }
}