using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiRoulette.Models
{
    public class Roulette
    {
        [Required]
        public int id { set; get; }
        [Required]
        public string name { set; get; }
        [Required]
        public DateTime create_at { set; get; }
        [Required]
        public string status { set; get; }
        [Required]
        public IList<Bet> bets_open { set; get; }

        public IList<Bet> bets_close { set; get; }

    }
}
