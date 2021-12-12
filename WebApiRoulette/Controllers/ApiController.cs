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
    public class ApiController : ControllerBase
    {

        [HttpPost("Roulette/create")]
        public long CreateRoulette()
        {
            try
            {
                var redisDB = connection.Connection.GetDatabase();
                int indexItem = redisDB.ListRange("Roulettes").Length + 1;
                Roulette newItem = new Roulette
                {
                    name = "Roulette." + indexItem,
                    create_at = DateTime.Now,
                    status = "Open"
                };
                long RouletteId = redisDB.ListRightPush("Roulettes", JsonConvert.SerializeObject(newItem));

                return RouletteId;

            }

            catch (Exception e)
            {

                return 0;
            }

        }

        [HttpPut("Roulette/open/{id}")]
        public bool Open(long id)
        {

            try
            {
                var redisDB = connection.Connection.GetDatabase();
                bool success = false;
                Roulette currentRoulette = GetRoulette(id);
                currentRoulette.status = "Open";
                if (currentRoulette.name != null)
                {
                    redisDB.ListSetByIndex("Roulettes", id - 1, JsonConvert.SerializeObject(currentRoulette));
                    success = true;
                }

                return success;
            }
            catch
            {
                return false;

            }

        }


        public Roulette GetRoulette(long id)
            {
                var redisDB = connection.Connection.GetDatabase();
                var currentItem = redisDB.ListRange("Roulettes", id - 1, id - 1);
                Roulette currentRoulette = new Roulette();

                if (currentItem.Length > 0)
                {
                    currentRoulette = JsonConvert.DeserializeObject<Roulette>(currentItem[0]);

                }

                return currentRoulette;
            }








        }
    }
