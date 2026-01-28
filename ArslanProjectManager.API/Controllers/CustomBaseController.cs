using ArslanProjectManager.Core.Constants;
using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace ArslanProjectManager.API.Controllers
{
    /// <summary>
    /// Base controller for API endpoints that use a consistent response format and shared validation helpers.
    /// </summary>
    /// <remarks>
    /// Provides <see cref="CreateActionResult{T}"/> for wrapping DTOs in <see cref="CustomResponseDto{T}"/>,
    /// <see cref="GetToken"/> for resolving the current request's token from header or cookie, and validation
    /// helpers for model, email, password, and id. <paramref name="tokenService"/> is used for token lookup.
    /// </remarks>
    /// <param name="tokenService">Service for resolving and validating access tokens.</param>
    public partial class CustomBaseController(ITokenService tokenService) : ControllerBase
    {
        /// <summary>
        /// Token service for derived controllers that need to call it directly (avoids CS9107 when also passed to base).
        /// </summary>
        protected ITokenService TokenService => tokenService;

        /// <summary>
        /// Wraps a <see cref="CustomResponseDto{T}"/> into an appropriate <see cref="IActionResult"/> (status code and body).
        /// </summary>
        /// <typeparam name="T">The type of the response data.</typeparam>
        /// <param name="response">The response DTO; if null, returns <see cref="ControllerBase.NotFound"/>.</param>
        /// <returns>NotFound if response is null; otherwise ObjectResult with the response's status code and body (null body for 204).</returns>
        [NonAction]
        public IActionResult CreateActionResult<T>(CustomResponseDto<T> response)
        {
            if (response is null)
            {
                return NotFound();
            }
            else if (response.StatusCode == 204)
            {
                return new ObjectResult(null) { StatusCode = response.StatusCode };
            }
            else
            {
                return new ObjectResult(response) { StatusCode = response.StatusCode };
            }
        }

        /// <summary>
        /// Gets the current token entity from the access token sent in the request (Authorization header or AccessToken cookie).
        /// </summary>
        /// <returns>The token from the database if found and valid; otherwise null.</returns>
        [NonAction]
        protected async Task<Token?> GetToken()
        {
            string? accessToken = null;
            var authHeader = HttpContext.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Bearer "))
            {
                accessToken = authHeader["Bearer ".Length..].Trim();
            }
            else if (HttpContext.Request.Cookies.ContainsKey("AccessToken"))
            {
                accessToken = HttpContext.Request.Cookies["AccessToken"];
            }

            if (string.IsNullOrWhiteSpace(accessToken))
            {
                return null;
            }

            return await TokenService.GetValidTokenByAccessTokenAsync(accessToken);
        }

        /// <summary>
        /// Validates a model with a custom predicate; if invalid, returns a 400 result with the given message.
        /// </summary>
        /// <typeparam name="T">The type of the model.</typeparam>
        /// <param name="model">The model to validate (can be null).</param>
        /// <param name="validator">Predicate that must return true for the model to be considered valid.</param>
        /// <param name="errorMessage">Message used in the 400 response when validation fails.</param>
        /// <returns>An IActionResult (400) when model is null or validator returns false; otherwise null (validation passed).</returns>
        [NonAction]
        protected IActionResult? ValidateModel<T>(T model, Func<T, bool> validator, string errorMessage)
        {
            if (model is null || !validator(model))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, errorMessage));
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Validates that the string is a non-empty, well-formed email address.
        /// </summary>
        /// <param name="email">The email string to validate.</param>
        /// <returns>An IActionResult (400) with <see cref="ErrorMessages.InvalidEmailFormat"/> when invalid; otherwise null.</returns>
        [NonAction]
        protected IActionResult? ValidateEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidEmailFormat));
            }

            var emailRegex = EmailFormatRegex();
            if (!emailRegex.IsMatch(email))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidEmailFormat));
            }

            return null;
        }

        /// <summary>
        /// Validates that the password meets strength requirements (length and character set).
        /// </summary>
        /// <param name="password">The password string to validate.</param>
        /// <returns>An IActionResult (400) with <see cref="ErrorMessages.InvalidPasswordFormat"/> when invalid; otherwise null.</returns>
        [NonAction]
        protected IActionResult? ValidatePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidPasswordFormat));
            }

            var passwordRegex = StrongPasswordRegex();
            if (!passwordRegex.IsMatch(password))
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, ErrorMessages.InvalidPasswordFormat));
            }

            return null;
        }

        /// <summary>
        /// Validates that the id is a positive integer.
        /// </summary>
        /// <param name="id">The id to validate.</param>
        /// <param name="errorMessage">Message used in the 400 response when id is less than or equal to zero.</param>
        /// <returns>An IActionResult (400) when id &lt;= 0; otherwise null.</returns>
        [NonAction]
        protected IActionResult? ValidateId(int id, string errorMessage)
        {
            if (id <= 0)
            {
                return CreateActionResult(CustomResponseDto<NoContentDto>.Fail(400, errorMessage));
            }
            else
            {
                return null;
            }
        }

        /// <summary>Regex for standard email format.</summary>
        [GeneratedRegex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$")]
        private partial Regex EmailFormatRegex();

        /// <summary>Regex for strong password (min length 8, upper, lower, digit, special).</summary>
        [GeneratedRegex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*\/?&+\-_.])[A-Za-z\d@$!%*\/?&+\-_.]{8,}$")]
        private partial Regex StrongPasswordRegex();
    }
}
