using ArslanProjectManager.Core.DTOs;
using ArslanProjectManager.Core.Models;
using ArslanProjectManager.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ArslanProjectManager.API.Filters
{
    public class NotFoundFilter<T>(IGenericService<T> service) : IAsyncActionFilter where T : BaseEntity
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var idValue = context.ActionArguments.Values.FirstOrDefault();
            if (idValue is not int id)
            {
                await next.Invoke();
                return;
            }

            var exists = await service.AnyAsync(x => x.Id == id);
            if (exists)
            {
                await next.Invoke();
                return;
            }

            context.Result = new NotFoundObjectResult(CustomResponseDto<NoContentDto>.Fail(404,
                $"{typeof(T).Name} Not Found."));
        }
    }
}
