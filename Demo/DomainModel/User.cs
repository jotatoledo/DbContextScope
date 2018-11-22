// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace Numero3.EntityFramework.Demo.DomainModel
{
    using System;

    // Anemic model to keep this demo application simple.
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int CreditScore { get; set; }
        public bool WelcomeEmailSent { get; set; }
        public DateTime CreatedOn { get; set; }

        public override string ToString()
        {
            return String.Format("Id: {0} | Name: {1} | Email: {2} | CreditScore: {3} | WelcomeEmailSent: {4} | CreatedOn (UTC): {5}", this.Id, this.Name, this.Email, this.CreditScore, this.WelcomeEmailSent, this.CreatedOn.ToString("dd MMM yyyy - HH:mm:ss"));
        }
    }
}
