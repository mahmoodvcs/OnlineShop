﻿using MahtaKala.Entities.ExceptionHandling;
using MahtaKala.Infrustructure.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Diagnostics;
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

        [Route("{*url}", Order = int.MaxValue - 10)]
        public ApiErrorResponse NotFoundError()
        {
            return new ApiErrorResponse(404, "HttpStatusCode", GetStatusCodeMessage(404));
        }

        static Dictionary<int, string> messages = new Dictionary<int, string>
        {
            {404, "آدرس پیدا نشد" }
        };

        private string GetStatusCodeMessage(int code)
        {
            if (messages.TryGetValue(code, out var msg))
                return msg;
            return ReasonPhrases.GetReasonPhrase(code);
        }

        [Route("error")]
        public ApiErrorResponse Error()
        {
            var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            if (exception is ApiException apiException)
            {
                Response.StatusCode = apiException.StatusCode;
                return new ApiErrorResponse(apiException);
            }
            else if (exception is UnauthorizedAccessException)
            {
                Response.StatusCode = 401;
            }
            else if (IsSqlException(exception, out var sqlex))
            {
                Response.StatusCode = 400;
                return new ApiErrorResponse(400, "SqlException", SqlErrorParsers.TranslateMessage(sqlex));
            }
            else
            {
                Response.StatusCode = 500;
            }
            return new ApiErrorResponse(exception, Response.StatusCode);
        }

        public bool IsSqlException(Exception ex, out SqlException sqlex)
        {
            while (ex != null && !(ex is SqlException))
            {
                ex = ex.InnerException;
            }
            sqlex = ex == null ? null : (SqlException)ex;
            return ex != null;
        }
    }
}
