using System.Text.Json.Serialization;

namespace Common
{
    public class Result
    {
        protected Result()
        {
        }

        [JsonPropertyName("message")]
        public string Message { get; protected set; }

        [JsonPropertyName("succeeded")]
        public bool Succeeded { get; protected set; }

        public static Result Fail()
        {
            return new Result
            {
                Succeeded = false
            };
        }

        public static Result Fail(string message)
        {
            return new Result
            {
                Succeeded = false,
                Message = message
            };
        }

        public static Result Success()
        {
            return new Result
            {
                Succeeded = true
            };
        }

        public static Result Success(string message)
        {
            return new Result
            {
                Succeeded = true,
                Message = message
            };
        }
    }

    public class Result<T> : Result
    {
        protected Result()
        {
        }

        [JsonPropertyName("data")]
        public T Data { get; private init; }

        public static Result<T> Fail()
        {
            return new Result<T>
            {
                Succeeded = false
            };
        }

        public static Result<T> Fail(string message)
        {
            return new Result<T>
            {
                Succeeded = false,
                Message = message
            };
        }

        public static Result<T> Success()
        {
            return new Result<T>
            {
                Succeeded = true
            };
        }

        public static Result<T> Success(string message)
        {
            return new Result<T>
            {
                Succeeded = true,
                Message = message
            };
        }

        public static Result<T> Success(T data)
        {
            return new Result<T>
            {
                Succeeded = true,
                Data = data
            };
        }

        public static Result<T> Success(T data, string message)
        {
            return new Result<T>
            {
                Succeeded = true,
                Message = message,
                Data = data
            };
        }
    }
}