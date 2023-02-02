using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Web;

namespace Plants.Presentation;

public class UrlAuthMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IConfiguration _config;

    public UrlAuthMiddleware(RequestDelegate next, IConfiguration config)
    {
        _next = next;
        _config = config;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var queryString = HttpUtility.ParseQueryString(context.Request.QueryString.Value);
            var token = queryString.Get("token");
            if (String.IsNullOrEmpty(token) == false)
            {
                var validationParameters = Infrastructure.DIExtensions.GetValidationParams(Infrastructure.DIExtensions.GetAuthKey(_config));

                var validator = new JwtSecurityTokenHandler();

                if (validator.CanReadToken(token))
                {
                    var principal = validator.ValidateToken(token, validationParameters, out var validatedToken);
                    context.User.AddIdentity((ClaimsIdentity)principal.Identity);
                }
            }
        }
        catch
        {

        }

        //invoking the next middleware 
        await _next.Invoke(context);
    }
}
