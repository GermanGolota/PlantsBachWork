using Swashbuckle.AspNetCore.Filters;
using static Plants.Presentation.AuthControllerV2;

namespace Plants.Presentation;

internal class LoginRequestExampleV2 : IExamplesProvider<LoginCommand>
{
    public LoginCommand GetExamples() =>
         new("postgres", "testPassword");
}
