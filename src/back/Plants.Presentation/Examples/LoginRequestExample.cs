using Plants.Application.Commands;
using Plants.Presentation.Controllers.v2;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation.Examples;

internal class LoginRequestExample : IExamplesProvider<LoginCommand>
{
    public LoginCommand GetExamples() =>
        new("postgres", "password");

}

internal class LoginRequestExamplev2 : IExamplesProvider<LoginDto>
{
    public LoginDto GetExamples() =>
         new("admin", "changeit");
}
