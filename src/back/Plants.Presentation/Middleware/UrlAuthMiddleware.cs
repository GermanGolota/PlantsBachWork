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
            var query = context.Request.QueryString.Value;
            if (query is not null)
            {
                var queryString = HttpUtility.ParseQueryString(query);
                var token = queryString.Get("token");
                if (String.IsNullOrEmpty(token) == false)
                {
                    var validationParameters = DIExtensions.GetValidationParams(DIExtensions.GetAuthKey(_config));

                    var validator = new JwtSecurityTokenHandler();

                    if (validator.CanReadToken(token))
                    {
                        var principal = validator.ValidateToken(token, validationParameters, out var validatedToken);
                        if (principal.Identities is ClaimsIdentity claims)
                        {
                            context.User.AddIdentity(claims);
                        }
                    }
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
