using Microsoft.Extensions.Configuration;
using System.Configuration;

namespace Example.Common.Utilities
{
    public class AppSettings
    {
        private static AppSettings _instance;
        private static readonly object ObjLocked = new object();
        private IConfiguration _configuration;

        protected AppSettings()
        {

        }

        public void SetConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SetConfiguration(string environmentName, Microsoft.Extensions.Configuration.ConfigurationManager configuration)
        {
            configuration.AddJsonFile(path: "appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile(path: $"appsettings.{environmentName}.json", optional: true, reloadOnChange: true)
                    .AddJsonFile(path: $"appsettings.Init.json", optional: true, reloadOnChange: true);

            _configuration = configuration;
        }

        /// <summary>
        /// The get application configuration
        /// Create Date: 6/21/2021 4:42 PM
        /// </summary>
        /// <param name="pstrKey">The PSTR key.</param>
        /// <param name="pstrType">Type of the PSTR.</param>
        /// <returns></returns>
        public static object GetAppConfig(string pstrKey, string pstrType)
        {
            try
            {
                var objAppReader = new AppSettingsReader();
                return objAppReader.GetValue(pstrKey, Type.GetType(pstrType));
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// The get application configuration
        /// Create Date: 6/21/2021 4:42 PM
        /// </summary>
        /// <param name="pstrKey">The PSTR key.</param>
        /// <returns></returns>
        public static string GetAppConfig(string pstrKey)
        {
            try
            {
                var objAppReader = new AppSettingsReader();
                return objAppReader.GetValue(pstrKey, typeof(string)).ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static AppSettings Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (ObjLocked)
                    {
                        if (null == _instance)
                            _instance = new AppSettings();
                    }
                }
                return _instance;
            }
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            try
            {
                return _configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key).Value.ToBool();
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetConnection(string key, string defaultValue = "")
        {
            try
            {
                return _configuration.GetConnectionString(key);
            }
            catch
            {
                return defaultValue;
            }
        }

        public int GetInt32(string key, int defaultValue = 0)
        {
            try
            {
                return _configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key).Value.ToInt();
            }
            catch
            {
                return defaultValue;
            }
        }

        public long GetInt64(string key, long defaultValue = 0L)
        {
            try
            {
                return (!string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings[key]) ? System.Configuration.ConfigurationManager.AppSettings[key].ToLong() : defaultValue);
            }
            catch
            {
                return defaultValue;
            }
        }

        public string GetString(string key, string defaultValue = "")
        {
            try
            {
                var value = _configuration.GetSection("StringValue").GetChildren().FirstOrDefault(x => x.Key == key)?.Value;
                return string.IsNullOrEmpty(value) ? defaultValue : value;
            }
            catch
            {
                return defaultValue;
            }
        }

        public T Get<T>(string key = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(key))
                    return _configuration.Get<T>();
                else
                {
                    T value = _configuration.GetSection(key).Get<T>();
                    return value;
                }
            }
            catch (System.Exception)
            {
                return default;
            }
        }

        public T Get<T>(string key, T defaultValue)
        {
            if (_configuration.GetSection(key) == null)
                return defaultValue;

            if (string.IsNullOrWhiteSpace(key))
                return _configuration.Get<T>();
            var value = _configuration.GetSection(key).Get<T>();
            return (value == null || EqualityComparer<T>.Default.Equals(value, default)) ? defaultValue : value;
        }

        public static T GetObject<T>(string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
                return Instance._configuration.Get<T>();
            else
            {
                var section = Instance._configuration.GetSection(key);
                return section.Get<T>();
            }
        }

        public static T GetObject<T>(string key, T defaultValue)
        {
            if (Instance._configuration.GetSection(key) == null)
                return defaultValue;

            if (string.IsNullOrWhiteSpace(key))
                return Instance._configuration.Get<T>();
            else
                return Instance._configuration.GetSection(key).Get<T>();
        }
    }
}
