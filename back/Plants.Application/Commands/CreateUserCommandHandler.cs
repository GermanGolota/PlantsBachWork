using MediatR;
using Plants.Application.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace Plants.Application.Commands
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreateUserResult>
    {
        private readonly IUserService _user;
        private readonly IEmailer _emailer;

        public CreateUserCommandHandler(IUserService user, IEmailer emailer)
        {
            _user = user;
            _emailer = emailer;
        }

        public async Task<CreateUserResult> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            const int TempPasswordLength = 8;
            var tempPassword = GetRandomAlphanumericString(TempPasswordLength);
            await _emailer.SendInvitationEmail(request.Email, request.Login, tempPassword);
            return await _user.CreateUser(request.Login, request.Roles, 
                request.FirstName, request.LastName, request.PhoneNumber, tempPassword);
        }

        private static string GetRandomAlphanumericString(int length)
        {
            const string alphanumericCharacters =
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "abcdefghijklmnopqrstuvwxyz" +
                "0123456789";
            return GetRandomString(length, alphanumericCharacters);
        }

        private static string GetRandomString(int length, IEnumerable<char> characterSet)
        {
            if (length < 0)
                throw new ArgumentException("length must not be negative", "length");
            if (length > int.MaxValue / 8) // 250 million chars ought to be enough for anybody
                throw new ArgumentException("length is too big", "length");
            if (characterSet == null)
                throw new ArgumentNullException("characterSet");
            var characterArray = characterSet.Distinct().ToArray();
            if (characterArray.Length == 0)
                throw new ArgumentException("characterSet must not be empty", "characterSet");

            var bytes = new byte[length * 8];
            new RNGCryptoServiceProvider().GetBytes(bytes);
            var result = new char[length];
            for (int i = 0; i < length; i++)
            {
                ulong value = BitConverter.ToUInt64(bytes, i * 8);
                result[i] = characterArray[value % (uint)characterArray.Length];
            }
            return new string(result);
        }
    }
}
