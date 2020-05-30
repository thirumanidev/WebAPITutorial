using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace WebAPITutorial.Entities
{
    public class MoviesActors
    {
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public int MovieId { get; set; }        
        public Movie Movie { get; set; }
        public string Character { get; set; }
        public int Order { get; set; }
    }
}
