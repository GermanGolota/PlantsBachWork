using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation;

internal class LoginRequestExample : IExamplesProvider<LoginCommand>
{
    public LoginCommand GetExamples() =>
         new("postgres", "testPassword");
}
