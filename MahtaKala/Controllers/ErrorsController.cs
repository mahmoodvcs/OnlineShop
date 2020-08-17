using MahtaKala.Infrustructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorsController : Controller
    {
        private readonly ILogger<ErrorsController> logger;

        public ErrorsController(ILogger<ErrorsController> logger)
        {
            this.logger = logger;
        }

        [Route("error")]
        public ApiErrorResponse Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            //logger.LogError(exception, "");
            if(exception is ApiException apiException)
            {
                Response.StatusCode = apiException.StatusCode;
            }
            else if (exception is UnauthorizedAccessException)
            {
                Response.StatusCode = 401;
            }
            else
            {
                Response.StatusCode = 500;
            }
            return new ApiErrorResponse(exception);
        }
    }
}
