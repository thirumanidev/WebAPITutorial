using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebAPITutorial.Validations;

namespace WebAPITutorial.Entities
{
    public class Genere
    {
        public int GenereId { get; set; }
        [Required]
        [StringLength(40)]
        [FirstLetterUpperCaseAttribute]
        public string Name { get; set; }
        public List<MoviesGeneres> MoviesGeneres{ get; set; }
    }
}
