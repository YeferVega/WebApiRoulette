using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
                    status = "Open",
                    id = indexItem,
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


        [HttpPut("Roulette/bet/{id}/{userid}")]
        public bool play(long id, int userId, Bet betCurrent)
        {

            try
            {
                var redisDB = connection.Connection.GetDatabase();
                var userCurrent = redisDB.ListRange("Users", userId - 1, userId - 1);
                bool success = false;
                betCurrent.typeBet = betCurrent.typeBet.ToLower();

                bool bet_validation = betCurrent.typeBet != null && Regex.IsMatch(betCurrent.typeBet, @"^[0-9]$|^[1-2][0-9]$|^3[0-6]$");

                if (userCurrent.Length > 0 && (betCurrent.typeBet == "black" || betCurrent.typeBet == "red" || bet_validation))
                {
                    User user = JsonConvert.DeserializeObject<User>(userCurrent[0]);
                    if (betCurrent.money < 10000 || betCurrent.money < user.money)
                    {
                        user.money = user.money - betCurrent.money;
                        redisDB.ListSetByIndex("Users", userId - 1, JsonConvert.SerializeObject(user));
                    }
                    Roulette currentRoulette = GetRoulette(id);
                    if (currentRoulette.name != null)
                    {
                        betCurrent.userId = userId;

                        if (currentRoulette.bets_open == null)
                        {
                            List<Bet> betlist = new List<Bet>();
                            betlist.Add(betCurrent);

                            currentRoulette.bets_open = betlist;
                        }
                        else
                        {
                            currentRoulette.bets_open.Add(betCurrent);
                        }


                        redisDB.ListSetByIndex("Roulettes", id - 1, JsonConvert.SerializeObject(currentRoulette));

                        success = true;
                    }

                    return success;

                }

                else
                {

                    return success;

                }

            }
            catch (Exception e)
            {
                return false;

            }

        }


        [HttpPut("Roulette/close/{id}")]
        public bool Close(long id)
        {

            try
            {
                var redisDB = connection.Connection.GetDatabase();
                bool success = false;
                Roulette currentRoulette = GetRoulette(id);
                currentRoulette.status = "Close";
                var rand = new Random();
                int winnernumber = rand.Next(0, 36);
                string winnerstring = winnernumber.ToString();
                double winnermoney;

                if (currentRoulette.name != null)
                {
                    foreach (Bet item in currentRoulette.bets_open)
                    {
                        item.winner = winnernumber;

                        if (winnerstring == item.typeBet)
                        {
                            winnermoney = item.money * 5;
                        }
                        else if (item.typeBet == "red" || winnernumber % 2 != 0)
                        {
                            winnermoney = item.money * 1.8;

                        }
                        else if (item.typeBet == "black" || winnernumber % 2 == 0)
                        {
                            winnermoney = item.money * 1.8;

                        }
                        else
                        {
                            winnermoney = 0;
                        }

                        if (currentRoulette.bets_close == null)
                        {
                            List<Bet> betlist = new List<Bet>();
                            betlist.Add(item);
                            currentRoulette.bets_close = betlist;
                        }
                        else
                        {
                            currentRoulette.bets_close.Add(item);
                        }

                        var userCurrent = redisDB.ListRange("Users", item.userId - 1, item.userId - 1);

                        if (userCurrent.Length > 0)
                        {
                            User user = JsonConvert.DeserializeObject<User>(userCurrent[0]);
                            user.money = winnermoney + user.money;
                            redisDB.ListSetByIndex("Users", item.userId - 1, JsonConvert.SerializeObject(user));
                        }



                    }

                    currentRoulette.bets_open.Clear();

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




        [HttpPost("User/create")]
        public long CreateUser(User user)
        {
            var redisDB = connection.Connection.GetDatabase();
            long indexItem = redisDB.ListRange("Users").Length + 1;
            user.id = indexItem;
            long id = redisDB.ListRightPush("Users", JsonConvert.SerializeObject(user));

            return id;
        }



        [HttpGet("Roulette/show")]
        public List<Roulette> ShowRoulette()
        {
            var redisDB = connection.Connection.GetDatabase();
            var currentItem = redisDB.ListRange("Roulettes", 0, -1);
            List<Roulette> listRulettes = new List<Roulette>();



            if (currentItem.Length > 0)
            {
                for (var i = 0; i < currentItem.Length; i++)
                {
                    Roulette item = JsonConvert.DeserializeObject<Roulette>(currentItem[i]);
                    listRulettes.Add(item);
                }
            }

            return listRulettes;
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
