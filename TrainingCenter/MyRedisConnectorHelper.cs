using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace TrainingCenter
{
    public class MyRedisConnectorHelper
    {
        static MyRedisConnectorHelper()
        {
            MyRedisConnectorHelper.lazyConnection = new Lazy<ConnectionMultiplexer>(() =>
            {
                //return ConnectionMultiplexer.Connect("192.168.75.11:6379");
                return ConnectionMultiplexer.Connect(System.Configuration.ConfigurationManager.AppSettings["Redis_Server"].ToString());
            });
        }

        private static Lazy<ConnectionMultiplexer> lazyConnection;

        public static ConnectionMultiplexer Connection
        {
            get
            {
                return lazyConnection.Value;
            }
        }
    }
}