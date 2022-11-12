﻿namespace Plants.Domain.Services;

public class CommandMetadataFactory
{
    private readonly IDateTimeProvider _dateTime;
    private readonly IUserNameProvider _userName;

    public CommandMetadataFactory(IDateTimeProvider dateTime, IUserNameProvider userName)
    {
        _dateTime = dateTime;
        _userName = userName;
    }

    public CommandMetadata Create<T>(AggregateDescription aggregate) where T : Command
    {
        var name = typeof(T).Name.Replace("Command", "");
        return new CommandMetadata(Guid.NewGuid(), aggregate, _dateTime.UtcNow, name, _userName.UserName);
    }
}
