using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    /// <summary>
    /// This is the base for all controllers that render HTML
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SiteControllerBase<T> : MahtaControllerBase<T>
        where T : SiteControllerBase<T>
    {
        public SiteControllerBase(DataContext dataContext, ILogger<T> logger) : base(dataContext, logger)
        {
        }
    }
}
