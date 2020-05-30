using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPITutorial.DTOs;
using WebAPITutorial.Entities;
using WebAPITutorial.Helpers;
using WebAPITutorial.Services;

namespace WebAPITutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "people";

        public PeopleController(ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService)
        {
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }
        [HttpGet]
        public async Task<ActionResult<List<PersonDTO>>> Get([FromQuery] PaginationDTO pagination){
            var queryable = context.People.AsQueryable();
            await HttpContext.InsertPaginationParamsIntoResponse(queryable, pagination.RecordsPerPage);

            var people = await queryable.Paginate(pagination).ToListAsync();
            
            return mapper.Map<List<PersonDTO>>(people);
        }
        
        [HttpGet("{PersonId:int}", Name = "getPersonById")]
        public async Task<ActionResult<PersonDTO>> Get(int PersonId)
        {
            var person = await context.People.FirstOrDefaultAsync(x => x.PersonId == PersonId);
            if (person == null)
            {
                return NotFound();
            }
            return mapper.Map<PersonDTO>(person);
        }

        [HttpPost]
        public async Task<ActionResult<PersonDTO>> Post([FromForm] PersonCreationDTO personCreationDTO)
        {
            var person = mapper.Map<Person>(personCreationDTO);
            if (personCreationDTO.Picture != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreationDTO.Picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(personCreationDTO.Picture.FileName);
                    person.Picture = await fileStorageService.SaveFile(content, extension, containerName, personCreationDTO.Picture.ContentType);
                }
            }
            context.Add(person);
            await context.SaveChangesAsync();
            var personDTO = mapper.Map<PersonDTO>(person);
            return new CreatedAtRouteResult("getPersonById", new { person.PersonId }, personDTO);
        }

        [HttpPut("{PersonId}")]
        public async Task<ActionResult> Put(int id, [FromForm] PersonCreationDTO personCreationDTO)
        {
            var personDB = await context.People.FirstOrDefaultAsync(x => x.PersonId == id);
            if (personDB == null) { return NotFound(); }
            personDB = mapper.Map(personCreationDTO, personDB);

            if (personCreationDTO.Picture != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await personCreationDTO.Picture.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(personCreationDTO.Picture.FileName);
                    personDB.Picture = await fileStorageService.EditFile(content, extension, containerName, personDB.Picture, personCreationDTO.Picture.ContentType);
                }
            }
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{PersonId}")]
        public async Task<ActionResult> Patch(int id, [FromBody] JsonPatchDocument<PersonPatchDTO> patchDocument)
        {
            if(patchDocument == null)
            {
                return BadRequest();
            }
            var entityFromDB = await context.People.FirstOrDefaultAsync(x => x.PersonId == id);
            if (entityFromDB == null)
            {
                return NotFound();
            }
            var entityDTO = mapper.Map<PersonPatchDTO>(entityFromDB);
            patchDocument.ApplyTo(entityDTO,ModelState);
            var isValid = TryValidateModel(entityDTO);
            if (!isValid)
            {
                return BadRequest(ModelState);
            }
            mapper.Map(entityDTO, entityFromDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{PersonId}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var Exists = await context.People.AnyAsync(x => x.PersonId == Id);
            if (!Exists)
            {
                return NotFound();
            }
            context.Remove(new Person() { PersonId = Id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}