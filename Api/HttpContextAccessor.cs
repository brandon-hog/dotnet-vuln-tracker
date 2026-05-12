using System.Security.Claims;

namespace Api;

public class HttpContextAccessor : IHttpContextAccessor
{
    public HttpContext? HttpContext { get; set; }

    public string? GetUserId()
    {
        return HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}