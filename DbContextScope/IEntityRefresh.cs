// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope
{
    internal interface IEntityRefresh
    {
        void Refresh<TEntity>(TEntity toRefresh);
    }
}
