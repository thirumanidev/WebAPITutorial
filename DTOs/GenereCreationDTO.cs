using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using WebAPITutorial.Validations;

namespace WebAPITutorial.DTOs
{
    public class GenereCreationDTO
    {
        [Required]
        [StringLength(40)]
        [FirstLetterUpperCaseAttribute]
        public string Name { get; set; }
    }
}
