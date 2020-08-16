using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Models
{
    public interface ILocationModel
    {
        double Lat { get; set; }
        double Lng { get; set; }
    }
}
