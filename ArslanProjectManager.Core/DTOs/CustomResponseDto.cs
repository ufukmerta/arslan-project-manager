using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ArslanProjectManager.Core.DTOs
{
    public class CustomResponseDto<T>
    {
        public T Data { get; set; } = default!;
        [JsonIgnore]
        public int StatusCode { get; set; }
        public bool IsSuccess { get; set; }
        public List<string>? Errors { get; set; }
        public static CustomResponseDto<T> Success(T data, int statusCode)
        {
            return new CustomResponseDto<T>
            {
                Data = data,
                StatusCode = statusCode,
                IsSuccess = true,
                Errors = null
            };
        }
        public static CustomResponseDto<T> Success(int statusCode)
        {
            return new CustomResponseDto<T>
            {
                StatusCode = statusCode,
                IsSuccess = true,
                Errors = null
            };
        }
        public static CustomResponseDto<T> Fail(int statusCode, List<string> errors)
        {
            return new CustomResponseDto<T>
            {
                StatusCode = statusCode,
                IsSuccess = false,
                Errors = errors
            };
        }
        public static CustomResponseDto<T> Fail(int statusCode, string error)
        {
            return new CustomResponseDto<T>
            {
                StatusCode = statusCode,
                IsSuccess = false,
                Errors = new List<string> { error }
            };
        }
    }
}
