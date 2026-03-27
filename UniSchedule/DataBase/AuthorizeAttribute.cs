// AuthorizeAttribute.cs
using Microsoft.AspNetCore.Mvc.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    private readonly string[] _allowedRoles;
    
    public AuthorizeAttribute(params string[] roles)
    {
        _allowedRoles = roles;
    }
    
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.Items["User"];
        var userRole = context.HttpContext.Items["UserRole"]?.ToString();
        
        if (user == null)
        {
            context.Result = new Microsoft.AspNetCore.Mvc.UnauthorizedResult();
            return;
        }
        
        if (_allowedRoles.Any() && !_allowedRoles.Contains(userRole))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.StatusCodeResult(StatusCodes.Status403Forbidden);
            return;
        }
    }
}
