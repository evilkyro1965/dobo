using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Kooboo.Runtime.Serialization;
namespace Kooboo.CMS.Content.Persistence.SqlServer
{
    public class ConnectionSetting
    {
        public string Name { get; set; }
        public string ConnectionString { get; set; }
    }

    public class SqlServerSettings
    {
        private static SqlServerSettings instance = null;
        static SqlServerSettings()
        {
            string settingFile = GetSettingFile();
            if (File.Exists(settingFile))
            {
                instance = DataContractSerializationHelper.Deserialize<SqlServerSettings>(settingFile);
            }
            else
            {
                instance = new SqlServerSettings()
                {
                    SharingDatabase = true,
                    SharingDatabaseConnectionString = "Server=.\\SQLExpress;Database=Kooboo_CMS; Trusted_Connection=Yes;",
                    CreateDatabaseSetting = "Server=.\\SQLExpress;Database=Kooboo_CMS; Trusted_Connection=Yes;",
                    Connections = new[] { new ConnectionSetting() { Name = "Sample", ConnectionString = "Server=.\\SQLExpress;Database=Kooboo_CMS; Trusted_Connection=Yes;" } }
                };
                instance.Save();
            }
        }
        public void Save()
        {
            string settingFile = GetSettingFile();
            DataContractSerializationHelper.Serialize<SqlServerSettings>(this, settingFile);
        }
        private static string GetSettingFile()
        {
            return Path.Combine(Kooboo.Settings.BinDirectory, "SqlServer.config");
        }
        public static SqlServerSettings Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
                instance.Save();
            }
        }

        public bool SharingDatabase { get; set; }
        public string CreateDatabaseSetting { get; set; }
        public ConnectionSetting[] Connections { get; set; }
        public ConnectionSetting GetConnection(string repositoryName)
        {
            if (Connections == null)
            {
                return null;
            }
            return Connections.Where(it => it.Name.EqualsOrNullEmpty(repositoryName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }
        public void AddConnection(ConnectionSetting connection)
        {
            if (Connections == null)
            {
                Connections = new[] { connection };
            }
            else
            {
                Connections = Connections.Concat(new[] { connection }).ToArray();
            }
        }
        public void RemoveConnection(string repositoryName)
        {
            if (Connections != null)
            {
                Connections = Connections.Where(it => !it.Name.EqualsOrNullEmpty(repositoryName, StringComparison.OrdinalIgnoreCase)).ToArray();
            }
        }
        /// <summary>
        /// Gets or sets the sharing database setting.
        /// if <paramref name="Individual"/> is false, all content repositories will store in a central database.
        /// </summary>
        /// <value>The sharing database setting.</value>
        public string SharingDatabaseConnectionString { get; set; }
    }
}
