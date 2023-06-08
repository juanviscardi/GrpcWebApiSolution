﻿using Common.Interfaces;
using System.Configuration;

namespace Common
{
    public class SettingsManager: ISettingsManager
    {
        public string ReadSetting(string key)
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                return appSettings[key] ?? string.Empty;
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error leyendo la configuracion");
                return string.Empty;
            }

        }
    }
}
