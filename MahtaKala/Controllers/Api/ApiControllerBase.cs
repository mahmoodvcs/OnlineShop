﻿using MahtaKala.Entities;
using MahtaKala.Infrustructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    public class ApiControllerBase<T> : MahtaControllerBase<T>
        where T : ApiControllerBase<T>
    {
        public ApiControllerBase(DataContext dataContext, ILogger<T> logger) : base(dataContext, logger)
        {
        }
    }
}
