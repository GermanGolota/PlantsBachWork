using Plants.Application.Commands;
using Swashbuckle.AspNetCore.Filters;

namespace Plants.Presentation;

internal class LoginRequestExample : IExamplesProvider<LoginCommand>
{
    public LoginCommand GetExamples() =>
        new("postgres", "password");

}

internal class LoginRequestExampleV2 : IExamplesProvider<LoginCommand>
{
    public LoginCommand GetExamples() =>
         new("postgres", "testPassword");
}
