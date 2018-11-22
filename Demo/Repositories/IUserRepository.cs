// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace Numero3.EntityFramework.Demo.Repositories
{
    using System;
    using System.Threading.Tasks;
    using Numero3.EntityFramework.Demo.DomainModel;

    public interface IUserRepository
    {
        User Get(Guid userId);
        Task<User> GetAsync(Guid userId);
        void Add(User user);
    }
}