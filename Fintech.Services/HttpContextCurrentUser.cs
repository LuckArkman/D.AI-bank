using System.Security.Claims;
using Fintech.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Fintech.Api.Services;

public class HttpContextCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _accessor;

    public HttpContextCurrentUser(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public Guid AccountId 
    {
        get 
        {
            var claim = _accessor.HttpContext?.User?.FindFirst("AccountId");
            if (claim == null) throw new UnauthorizedAccessException("Token sem AccountId.");
            return Guid.Parse(claim.Value);
        }
    }
}