using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPITutorial.Entities
{
    public class MoviesGeneres
    {
        public int MovieId { get; set; }
        public int GenereId { get; set; }
        public Movie Movie { get; set; }
        public Genere Genere { get; set; }
    }
}
