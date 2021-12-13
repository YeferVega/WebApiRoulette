using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiRoulette.Utils
{
    public class redisDB
    {
        //creation of the construct
        public class connection
        {
            //Creation of an static object using Lazy and ConnectionMultiplexer in order to create a constructor

            private static Lazy<ConnectionMultiplexer> _lazyConnection;

            //Get the value of the connection 
            public static ConnectionMultiplexer Connection
            {
                get
                {

                    return _lazyConnection.Value;
                }
            }

            //Instantiate an object in order to get the connection
            static connection()
            {

                _lazyConnection = new Lazy<ConnectionMultiplexer>(() => ConnectionMultiplexer.Connect("localhost"));
            }

        }


    }
}
