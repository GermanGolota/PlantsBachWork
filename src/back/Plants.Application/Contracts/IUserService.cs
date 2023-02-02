﻿using Plants.Application.Commands;
using Plants.Application.Requests;
using Plants.Shared.Model;

namespace Plants.Application.Contracts;

public interface IUserService
{
    Task<IEnumerable<FindUsersResultItem>> SearchFor(string FullName, string Contact, UserRole[] Roles);
    Task RemoveRole(string login, UserRole role);
    Task AddRole(string login, UserRole role);
    Task<CreateUserResult> CreateUser(string Login, List<UserRole> Roles, string FirstName, 
        string LastName, string PhoneNumber, string Password);

    Task ChangeMyPassword(string newPassword);
}
