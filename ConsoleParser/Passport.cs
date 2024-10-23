using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleParser
{
    public class Passport
    {
        public int Id { get; set; }
        [Column("passp_series")]
        public int PASSP_SERIES { get; set; }
        [Column("passp_number")]
        public int PASSP_NUMBER { get; set; }
    }
}
