using System;
using System.Collections.Generic;

namespace LTC2.Shared.Utils.Bootstrap.Interfaces
{
    public interface ISettingsService
    {
        public Dictionary<Type, object> GetSettings();

        public TSettingsType GetSettings<TSettingsType>() where TSettingsType : class;
    }
}
