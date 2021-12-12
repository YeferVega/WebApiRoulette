using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebApiRoulette.Models
{
    public class User
    {
        public long id { set; get; }
        [Required]
        public string full_name { set; get; }
        [Required]
        public string email { set; get; }
        [Required]
        public  long money { set; get; }
    }
}
