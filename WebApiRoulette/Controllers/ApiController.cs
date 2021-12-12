using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using WebApiRoulette.Models;
using static WebApiRoulette.Utils.redisDB;


namespace WebApiRoulette.Controllers
{

    [ApiController]
    [Route("[Controller]")]
    public class ApiController: ControllerBase
    {

        [HttpPost("Roulette/create")]
        public long CreateRoulette()
        {
            try
            {
                var redisDB = connection.Connection.GetDatabase();
                int  indexItem = redisDB.ListRange("Roulette").Length+1;
                Roulette newItem = new Roulette
                {
                    name = "Roulette." + indexItem,
                    create_at = DateTime.Now,
                    status = "Open"
                };
                long RouletteId = redisDB.ListRightPush("Roulette", JsonConvert.SerializeObject(newItem));

                return RouletteId;

            }

            catch(Exception e)
            {
                return 0;
                
            }                 

        }

        




    }
}
