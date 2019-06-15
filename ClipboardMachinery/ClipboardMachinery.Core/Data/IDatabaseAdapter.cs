using System;
using System.Data;

namespace ClipboardMachinery.Core.Data {

    public interface IDatabaseAdapter : IDisposable {

        /// <summary>
        /// A connection to the database.
        /// If there is no previous connection or it was closed or broken, accessing this opens a new one.
        /// </summary>
        IDbConnection Connection { get; }

        /// <summary>
        /// Returns true whenever connection to the database is open, otherwise returns false.
        /// </summary>
        bool IsConnectionOpen { get; }

        /// <summary>
        /// User version information from database PRAGMA table.
        /// </summary>
        int Version { get; }

    }

}
