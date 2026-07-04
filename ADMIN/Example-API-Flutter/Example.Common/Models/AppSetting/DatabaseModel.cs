using System;
using System.Collections.Generic;
using System.Text;

namespace Example.Common.Models.AppSetting
{
    public class DatabaseModel
    {
        public ConnectionStringsModel PostgreSql { get; set; }
        public ConnectionStringsModel MySql { get; set; }
        public ConnectionStringsModel MSSQL { get; set; }
        //public ElasticSearchModel ElasticSearch { get; set; }
        //public RedisModel Redis { get; set; }
    }


    public class DbConfig
    {
        public string PackageName { get; set; }
        public List<DbInfo> ListDb { get; set; }
    }

    public class DbInfo
    {
        public bool IsMaster { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string Database { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public int Dbtype { get; set; }
    }
}
