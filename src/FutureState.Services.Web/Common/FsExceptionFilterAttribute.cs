using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using NLog;

namespace FutureState.Services.Web
{
    public class FsExceptionFilterAttribute : ExceptionFilterAttribute
    {
        static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;

            if(exception != null)
                if(_logger.IsErrorEnabled)
                    _logger.Error(exception);

            context.Result = new JsonResult(new JsonResponseDto() { Code = 500, Message = exception.Message });
        }
    }
}
