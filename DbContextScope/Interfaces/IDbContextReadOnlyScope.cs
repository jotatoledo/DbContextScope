// Copyright © Mehdi El Gueddari, José Toledo Navarro.
//
// This software may be modified and
// distributed under the terms of the MIT license.
// See the LICENSE file for details.

namespace EntityFrameworkCore.DbContextScope
{
    using System;

    /// <summary>
    /// A read-only DbContextScope. Refer to the comments for IDbContextScope
    /// for more details.
    /// </summary>
    public interface IDbContextReadOnlyScope : IDisposable
    {
        /// <summary>
        /// Gets the context instances that this manages.
        /// </summary>
        IDbContextCollection DbContexts { get; }
    }
}