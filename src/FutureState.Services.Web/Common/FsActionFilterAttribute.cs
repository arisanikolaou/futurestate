using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace FutureState.Services.Web
{
    public class FsActionFilterAttribute : ActionFilterAttribute, IExceptionFilter
    {
        static readonly Logger _logger = LogManager.GetLogger("FutureState.Services.Web");

        public void OnException(ExceptionContext context)
        {
            // todo: add error logging policy

            var exception = context.Exception as RuleException;
            if (exception != null)
            {
                // return rule error
                var ruleException = exception;

                context.Result = new JsonResult(
                    new JsonResponseDto()
                    {
                        Code = 500,
                        Rules = ruleException.Errors.Select(m => m.Message).ToArray(),
                        Message = exception.Message
                    });
            }
            else
            {
                _logger.Error(context.Exception, $"Unhandled error: {context.Exception?.Message}.");
            }
        }
    }
}