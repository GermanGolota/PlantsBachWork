using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation;

internal class LoginRequestExample : IExamplesProvider<AuthController.LoginViewRequest>
{
    public AuthController.LoginViewRequest GetExamples() =>
         new("superuser", "testPassword");
}
