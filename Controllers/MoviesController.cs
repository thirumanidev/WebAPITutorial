using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using WebAPITutorial.DTOs;
using WebAPITutorial.Entities;
using WebAPITutorial.Helpers;
using WebAPITutorial.Services;

namespace WebAPITutorial.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly ApplicationDbContext context;
        private readonly IMapper mapper;
        private readonly IFileStorageService fileStorageService;
        private readonly string containerName = "movies";

        public MoviesController( ApplicationDbContext context, IMapper mapper, IFileStorageService fileStorageService ){
            this.context = context;
            this.mapper = mapper;
            this.fileStorageService = fileStorageService;
        }

        [HttpGet]
        public async Task<ActionResult<List<MovieDTO>>> Get()
        {
            var movies = await context.Movies.ToListAsync();
            return mapper.Map<List<MovieDTO>>(movies);
        }

        /*[HttpGet("{MovieId}", Name = "getMovie")]
        public async Task<ActionResult<MovieDTO>> Get(int id)
        {
            var movie = await context.Movies.FirstOrDefaultAsync(x => x.MovieId == id);
            if (movie == null)
            {
                return NotFound();
            }
            return mapper.Map<MovieDTO>(movie);
        }*/

        [HttpGet("{MovieId}", Name = "getMovie")]
        public async Task<ActionResult<MovieDetailsDTO>> Get(int MovieId)
        {
            var movie = await context.Movies
                .Include(x => x.MoviesActors).ThenInclude(x => x.Movie)
                .Include(x => x.MoviesGeneres).ThenInclude(x => x.Genere)
                .FirstOrDefaultAsync(x => x.MovieId == MovieId);
            if (movie == null)
            {
                return NotFound();
            }
            return mapper.Map<MovieDetailsDTO>(movie);
        }

        [HttpGet]
        [Route("indexPageMovies")]
        public async Task<ActionResult<IndexPageMovies>> GetIndexPageMovies()
        {
            //var movie = await context.Movies.FirstOrDefaultAsync(x => x.MovieId == id);

            var top = 6;
            var today = DateTime.Today;
            var upcomingReleases = await context.Movies
                .Where(x => x.ReleaseDate > today)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();
            var inTheaters = await context.Movies
                .Where(x => x.InTheatears == true)
                .OrderBy(x => x.ReleaseDate)
                .Take(top)
                .ToListAsync();
            var result = new IndexPageMovies();
            result.UpcomingReleases = mapper.Map<List<MovieDTO>>(upcomingReleases);
            result.InTheaters = mapper.Map<List<MovieDTO>>(inTheaters);
            
            return result;
        }

        [HttpGet("filter")]
        public async Task<ActionResult<List<MovieDTO>>> Filter([FromQuery] MoviesFilterDTO moviesFilterDTO)
        {
            var moviesQueryable = context.Movies.AsQueryable();
            if (!string.IsNullOrWhiteSpace(moviesFilterDTO.Title))
            {
                moviesQueryable = moviesQueryable.Where(x => x.Title.Contains(moviesFilterDTO.Title));
            }
            if (moviesFilterDTO.InTheatears)
            {
                moviesQueryable = moviesQueryable.Where(x => x.InTheatears);
            }
            if (moviesFilterDTO.UpcomingReleases)
            {
                var today = DateTime.Today;
                moviesQueryable = moviesQueryable.Where(x => x.ReleaseDate > today);
            }
            if (moviesFilterDTO.GenereId != 0)
            {
                moviesQueryable = moviesQueryable.Where(x => x.MoviesGeneres.Select(y => y.GenereId).Contains(moviesFilterDTO.GenereId));
            }
            await HttpContext.InsertPaginationParamsIntoResponse(moviesQueryable, moviesFilterDTO.RecordsPerPage);
            var movies = await moviesQueryable.Paginate(moviesFilterDTO.Pagination).ToListAsync();
            return mapper.Map<List<MovieDTO>>(movies);
        }

        [HttpPost]
        public async Task<ActionResult> Post([FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movie = mapper.Map<Movie>(movieCreationDTO);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movie.Poster = await fileStorageService.SaveFile(content, extension, containerName, movieCreationDTO.Poster.ContentType);
                }
            }
            AnnotateActorsOrder(movie);
            context.Add(movie);
            await context.SaveChangesAsync();
            var movieDTO = mapper.Map<MovieDTO>(movie);
            return new CreatedAtRouteResult("getMovie", new { movie.MovieId }, movieDTO);
        }
        private static void AnnotateActorsOrder(Movie movie)
        {
            if (movie.MoviesActors != null)
            {
                for (int i = 0; i < movie.MoviesActors.Count; i++)
                {
                    movie.MoviesActors[i].Order = i;
                }
            }
        }
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromForm] MovieCreationDTO movieCreationDTO)
        {
            var movieDB = await context.Movies.FirstOrDefaultAsync(x => x.MovieId == id);
            if (movieDB == null) { return NotFound(); }
            movieDB = mapper.Map(movieCreationDTO, movieDB);

            if (movieCreationDTO.Poster != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await movieCreationDTO.Poster.CopyToAsync(memoryStream);
                    var content = memoryStream.ToArray();
                    var extension = Path.GetExtension(movieCreationDTO.Poster.FileName);
                    movieDB.Poster = await fileStorageService.EditFile(content, extension, containerName, movieDB.Poster, movieCreationDTO.Poster.ContentType);
                }
            }
            await context.Database.ExecuteSqlInterpolatedAsync($"Delete from MoviesActors where MovieId = {movieDB.MovieId}; Delete from MoviesGeneres where MovieId = {movieDB.MovieId};");
            AnnotateActorsOrder(movieDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{MovieId}")]
        public async Task<ActionResult> Patch(int MovieId, [FromBody] JsonPatchDocument<MoviePatchDTO> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }
            var entityFromDB = await context.Movies.FirstOrDefaultAsync(x => x.MovieId == MovieId);
            if (entityFromDB == null)
            {
                return NotFound();
            }
            var entityDTO = mapper.Map<MoviePatchDTO>(entityFromDB);
            patchDocument.ApplyTo(entityDTO, ModelState);
            var isValid = TryValidateModel(entityDTO);
            if (!isValid)
            {
                return BadRequest(ModelState);
            }
            mapper.Map(entityDTO, entityFromDB);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{MovieId}")]
        public async Task<ActionResult> Delete(int MovieId)
        {
            var Exists = await context.Movies.AnyAsync(x => x.MovieId == MovieId);
            if (!Exists)
            {
                return NotFound();
            }
            context.Remove(new Movie() { MovieId = MovieId });
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}