﻿using MediatR;
using Plants.Core;
using System.Collections.Generic;

namespace Plants.Application.Commands
{
    public record CreateUserCommand(string Login, List<UserRole> Roles, string Email,
        string FirstName, string LastName, string PhoneNumber) : IRequest<CreateUserResult>;

    public record CreateUserResult(bool Success, string Message);
}