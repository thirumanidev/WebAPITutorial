using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using WebAPITutorial.DTOs;
using WebAPITutorial.Entities;
using WebAPITutorial.Filters;
using WebAPITutorial.Services;

namespace WebAPITutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenereController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;

        public GenereController(ApplicationDbContext _context, IMapper mapper)
        {
            this.context = _context;
            this.mapper = mapper;
        }
        [HttpGet]
        public async Task<ActionResult<List<GenereDTO>>> Get()
        {
            var generes = await context.Generes.AsNoTracking().ToListAsync();
            var generesDTOs = mapper.Map<List<GenereDTO>>(generes);
            return generesDTOs;
        }
        [HttpGet("{GenereId:int}", Name = "getGenereById")]
        public async Task<IActionResult> Get(int Id)
        {
            var genere = await context.Generes.FirstOrDefaultAsync(x=> x.GenereId==Id);
            if (genere == null)
            {
                return NotFound();
            }
            // I think, here we don't need to Map this, coz we are returning IActionResult only. After tested: both return types are working..
            var genereDTO = mapper.Map<GenereDTO>(genere);
            return Ok(genereDTO);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromBody] GenereCreationDTO genereCreationDTO)
        {
            var genere = mapper.Map<Genere>(genereCreationDTO);
            context.Add(genere);
            await context.SaveChangesAsync();
            var genereDTO = mapper.Map<GenereDTO>(genere);
            return new CreatedAtRouteResult("getGenereById", new { genereDTO.GenereId }, genereDTO);
        }
        [HttpPut("{MovieId}")]
        public async Task<ActionResult> Put(int Id, [FromBody] GenereCreationDTO genereCreationDTO)
        {
            var genere = mapper.Map<Genere>(genereCreationDTO);
            genere.GenereId = Id;
            context.Entry(genere).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return NoContent();
        }
        [HttpDelete("{MovieId}")]
        public async Task<ActionResult> Delete(int Id)
        {
            var Exists = await context.Generes.AnyAsync(x => x.GenereId == Id);
            if (!Exists)
            {
                return NotFound();
            }
            context.Remove(new Genere() { GenereId = Id });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}