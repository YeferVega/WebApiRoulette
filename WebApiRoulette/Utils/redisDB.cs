using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiRoulette.Utils
{
    public class redisDB
    {
        public class connection
        {
            private static Lazy<ConnectionMultiplexer> _lazyConnection;
            public static ConnectionMultiplexer Connection
            {
                get {          
                    
                    return _lazyConnection.Value;
                }
            }
            static connection() {

                _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("localhost"));
            }

        }
        
        
    }
}
