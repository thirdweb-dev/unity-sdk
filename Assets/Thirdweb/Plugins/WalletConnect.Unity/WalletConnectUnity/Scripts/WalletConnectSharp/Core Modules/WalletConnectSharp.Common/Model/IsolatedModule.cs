using System;
using System.Collections.Generic;

namespace WalletConnectSharp.Common.Model
{
    /// <summary>
    /// A mock module that represents nothing. Useful for creating an empty
    /// context
    /// </summary>
    public sealed class IsolatedModule : IModule
    {
        private static HashSet<Guid> activeModules = new HashSet<Guid>();

        private Guid _guid;

        public string Name
        {
            get
            {
                return $"isolated-module-{Context}";
            }
        }

        public string Context
        {
            get
            {
                return $"IM-{_guid.ToString()}";
            }
        }

        public IsolatedModule()
        {
            do
            {
                _guid = Guid.NewGuid();
            } while (activeModules.Contains(_guid));

            activeModules.Add(_guid);
        }
    }
}