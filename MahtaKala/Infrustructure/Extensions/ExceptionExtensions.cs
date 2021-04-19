using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MahtaKala.Infrustructure.Extensions
{
	public static class ExceptionExtensions
	{
		public static string DigOutWholeExceptionMessage(this Exception ex)
		{
			Exception digger = ex;
			string errorMessage = ex.Message;
			while (digger.InnerException != null)
			{
				digger = digger.InnerException;
				errorMessage += Environment.NewLine + "|||||###--Digging one level deepr into the exception maze hole! Inner exception message follows..." + Environment.NewLine +
					digger.Message;
			}
			return errorMessage;
		}
	}
}
