// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace Numero3.EntityFramework.Demo.CommandModel
{
    using System;

    /// <summary>
    /// Specifications of the CreateUser command. Defines the properties of a new user.
    /// </summary>
    public class UserCreationSpec
    {
        /// <summary>
        /// The Id automatically generated for this user.
        /// </summary>
        public Guid Id { get; protected set; }

        public string Name { get; protected set; }
        public string Email { get; protected set; }

        public UserCreationSpec(string name, string email)
        {
            this.Id = Guid.NewGuid();
            this.Name = name;
            this.Email = email;
        }

        public void Validate()
        {
            // [...]
        }
    }
}
