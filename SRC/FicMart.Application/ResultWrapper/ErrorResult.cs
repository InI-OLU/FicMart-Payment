using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.ResultWrapper
{
    public class ErrorResult
    {
        public string Message { get; }
        public int StatusCode { get; }

        private ErrorResult(string message, int statusCode)
        {
            Message = message;
            StatusCode = statusCode;
        }

        public static ErrorResult NotFound(string message) => new ErrorResult(message, 404);
        public static ErrorResult BadRequest(string message) => new ErrorResult(message, 400);
        public static ErrorResult Unauthorized(string message) => new ErrorResult(message, 401);
        public static ErrorResult Forbidden(string message) => new ErrorResult(message, 403);
        public static ErrorResult InternalServerError(string message) => new ErrorResult(message, 500);
    }
}
