using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPITutorial.DTOs
{
    public class MovieDetailsDTO:MovieDTO
    {
        public List<GenereDTO> Generes { get; set; }
        public List<ActorDTO> Actors { get; set; }
    }
}
