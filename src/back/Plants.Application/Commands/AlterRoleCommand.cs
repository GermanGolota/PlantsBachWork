﻿using MediatR;
using Plants.Shared.Model;

namespace Plants.Application.Commands;

public record AlterRoleCommand(string Login, UserRole Role, AlterType Type) : IRequest<AlterRoleResult>;
public record AlterRoleResult(bool Successfull);

public enum AlterType
{
    Remove, Add
};
