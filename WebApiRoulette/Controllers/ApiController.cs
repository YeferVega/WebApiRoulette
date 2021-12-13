using System;
using System.Collections.Generic;
using System.Globalization;
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

        //Creation of the Endpoints .

        [HttpPost("Roulette/create")] 
        public long CreateRoulette()
        {
            try
            {
                //Use the class in order to connect with the database Redis .
                var redisDB = connection.Connection.GetDatabase();
                //Get the index in order to the variable save more late   .           
                int indexItem = redisDB.ListRange("Roulettes").Length + 1;
                //creation of the object type roulette with the values .
                Roulette newItem = new Roulette
                {
                    name = "Roulette." + indexItem,
                    // Convert datetime to string witht he format ISO 8601
                    create_at = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    status = "Open",
                    id = indexItem,
                };
                //The object has been serialize and send in order to save in the database.
                long RouletteId = redisDB.ListRightPush("Roulettes", JsonConvert.SerializeObject(newItem));

                //Return the id of the created item.
                return RouletteId;
            }

            catch (Exception e)
            {
                //Return 0 if an error occurs.
                return 0;
            }
        }

        [HttpPut("Roulette/open/{id}")]
        public bool Open(long id)
        {
            try
            {     
                //Use the class in order to connect with the database Redis .
                var redisDB = connection.Connection.GetDatabase();
                //Creation of the variable in order to validate if this process have been success.
                bool success = false;
                //Use of the method GetRoulette in order to get the Roulette .
                Roulette currentRoulette = GetRoulette(id);
                currentRoulette.status = "Open";
                //Validation if the object is not null.
                if (currentRoulette.name != null)
                {
                    //Save the new status of the roulettes.
                    redisDB.ListSetByIndex("Roulettes", id - 1, JsonConvert.SerializeObject(currentRoulette));
                    // Assign the value true, in order to return indicate that process ,it has been success.
                    success = true;
                }

                return success;
            }
            catch
            {
                //Return false if an error occurs.
                return false;
            }

        }

        [HttpPut("Roulette/bet/{id}/{userid}")]
        public bool play(long id, int userId, Bet betCurrent)
        {
            try
            {
                //Use the class in order to connect with the database Redis .
                var redisDB = connection.Connection.GetDatabase();
                //Get the information of the user.
                var userCurrent = redisDB.ListRange("Users", userId - 1, userId - 1);
                //Creation of the variable in order to validate if this process have been success.
                bool success = false;
                //Change the string to lower case.
                betCurrent.typeBet = betCurrent.typeBet.ToLower();
                //Validate the value input if this is number and it is between 0 to 36
                bool bet_validation = betCurrent.typeBet != null && Regex.IsMatch(betCurrent.typeBet, @"^[0-9]$|^[1-2][0-9]$|^3[0-6]$");
                //Validate of the inputs values.
                if (userCurrent.Length > 0 && (betCurrent.typeBet == "black" || betCurrent.typeBet == "red" || bet_validation))
                {
                    // Assign the player's values ​​to the new variable.
                    User user = JsonConvert.DeserializeObject<User>(userCurrent[0]);
                    //Check the player's money the max bet and money available
                    if (betCurrent.money < 10000 || betCurrent.money < user.money)
                    {
                        //Reduce the money bet and save the information.
                        user.money = user.money - betCurrent.money;
                        redisDB.ListSetByIndex("Users", userId - 1, JsonConvert.SerializeObject(user));
                    }

                    //Use of the method GetRoulette in order to get the Roulette .
                    Roulette currentRoulette = GetRoulette(id);

                    //Valdation of the status and the roulette exists
                    if (currentRoulette.name != null || currentRoulette.status!="open")
                    {
                        betCurrent.userId = userId;

                        //Creation of the list of the best open 
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

                        //Save the new information of the roulettes.
                        redisDB.ListSetByIndex("Roulettes", id - 1, JsonConvert.SerializeObject(currentRoulette));
                        // Assign the value true, in order to return indicate that process ,it has been success.
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
                //Return 0 if an error occurs.
                return false;

            }

        }

        [HttpPut("Roulette/close/{id}")]
        public bool Close(long id)
        {

            try
            {   
                //Use the class in order to connect with the database Redis .
                var redisDB = connection.Connection.GetDatabase();
                bool success = false;
                //Use of the method GetRoulette in order to get the Roulette .
                Roulette currentRoulette = GetRoulette(id);
                //Change of status of the Roulette
                currentRoulette.status = "Close";
                //Creation of the object of the class random.
                var rand = new Random();
                //Assing to values random to the variable winnernumber.
                int winnernumber = rand.Next(0, 36);
                //Assing to values random to the variable string in order to validate the colors.
                string winnerstring = winnernumber.ToString();
                //Creation of the variable in order to receive the earned money .
                double winnermoney;

                if (currentRoulette.name != null)
                {
                    //creation of a loop in order to validate the winners and assign the new money.
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

                       //Change the status of the bets and sent the information of the list bets_open to bets_close.
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

                        //Update the information of the user with the new amount.
                        var userCurrent = redisDB.ListRange("Users", item.userId - 1, item.userId - 1);

                        if (userCurrent.Length > 0)
                        {
                            User user = JsonConvert.DeserializeObject<User>(userCurrent[0]);
                            user.money = winnermoney + user.money;
                            redisDB.ListSetByIndex("Users", item.userId - 1, JsonConvert.SerializeObject(user));
                        }

                    }
                    //Remove all items of the list bets_open , those have moved of the list bets_close
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
            //Use the class in order to connect with the database Redis .
            var redisDB = connection.Connection.GetDatabase();
            //Create the user with the parameters send.
            long indexItem = redisDB.ListRange("Users").Length + 1;
            user.id = indexItem;
            long id = redisDB.ListRightPush("Users", JsonConvert.SerializeObject(user));

            return id;
        }

        [HttpGet("Roulette/show")]
        public List<Roulette> ShowRoulette()
        {
            //Use the class in order to connect with the database Redis .
            var redisDB = connection.Connection.GetDatabase();
            //Use the indice 0 to -1 get all elements from Redis 
            var currentItem = redisDB.ListRange("Roulettes", 0, -1);
            //Create a list in order to save the data got of  Redis.
            List<Roulette> listRulettes = new List<Roulette>();
            if (currentItem.Length > 0)
            {
                //Use of the loop in order to deserealized each item in order to save in the list.
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
            //Use the class in order to connect with the database Redis .
            var redisDB = connection.Connection.GetDatabase();
            // Reduce 1 of each index, Redis enumerates  since the number 0
            var currentItem = redisDB.ListRange("Roulettes", id - 1, id - 1);
            // Create new variable and save with the information deserealized.
            Roulette currentRoulette = new Roulette();
            if (currentItem.Length > 0)
            {
                currentRoulette = JsonConvert.DeserializeObject<Roulette>(currentItem[0]);
            }
            //Return the object with the data of the roulette if got the information of Redis or the empty if it has been occurred error.

            return currentRoulette;
        }
    }
}
