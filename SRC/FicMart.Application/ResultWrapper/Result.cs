using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FicMart.Application.ResultWrapper
{
   public class Result<T>
    {
        public bool IsSuccess { get; }
        public T Value { get; }
        public ErrorResult Error { get; }
        
        private Result (bool isSuccess, T value, ErrorResult error)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
        }

        public static Result<T> Success(T value) => new Result<T>(true, value, null);
        public static Result<T> Failure(ErrorResult error) => new Result<T>(false, default(T), error);

    }
}
