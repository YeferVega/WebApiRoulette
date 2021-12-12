using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiRoulette.Models
{
    public class Bet
    {
        [Required]
        public int id { set; get; }
        [Required]
        public long money { set; get; }
        [Required]
        public string typeBet { set; get; }
        [Required]
        public long userId { set; get; }

    }
}
