namespace Plants.Aggregates;

public static class UserPasswordValidator
{
    public static CommandForbidden? Validate(string password)
    {
        CommandForbidden? result;
        if (password.Length <= 6)
        {
            result = new CommandForbidden("Password is too short");
        }
        else
        {
            result = null;
        }
        return result;
    }
}
