using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPITutorial.DTOs;
using WebAPITutorial.Entities;

namespace WebAPITutorial.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Genere, GenereDTO>().ReverseMap();
            CreateMap<GenereCreationDTO, Genere>();
            CreateMap<Person, PersonDTO>().ReverseMap();
            CreateMap<PersonCreationDTO, Person>().ForMember(x => x.Picture, options => options.Ignore());
            CreateMap<Person, PersonPatchDTO>().ReverseMap();
            CreateMap<Movie, MovieDTO>().ReverseMap();
            CreateMap<MovieCreationDTO, Movie>()
                .ForMember(x => x.Poster, options => options.Ignore())
                .ForMember(x => x.MoviesGeneres, options => options.MapFrom(MapMoviesGeneres))
                .ForMember(x => x.MoviesActors, options => options.MapFrom(MapMoviesActors));
            CreateMap<Movie, MovieDetailsDTO>()
                .ForMember(x => x.Generes, options => options.MapFrom(MapMoviesGeneres))
                .ForMember(x => x.Actors, options => options.MapFrom(MapMoviesActors));
            CreateMap<Movie, MoviePatchDTO>().ReverseMap();
        }
        private List<MoviesGeneres> MapMoviesGeneres(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesGeneres>();
            foreach (var Id in movieCreationDTO.GenereIds)
            {
                result.Add(new MoviesGeneres() { GenereId = Id });
            }
            return result;
        }
        private List<MoviesActors> MapMoviesActors(MovieCreationDTO movieCreationDTO, Movie movie)
        {
            var result = new List<MoviesActors>();
            foreach (var actor in movieCreationDTO.Actors)
            {
                result.Add(new MoviesActors() { PersonId = actor.PersonId, Character= actor.Character });
            }
            return result;
        }
        private List<GenereDTO> MapMoviesGeneres(Movie movie, MovieDetailsDTO movieDetailsDTO)
        {
            var result = new List<GenereDTO>();
            foreach (var movieGenere in movie.MoviesGeneres)
            {
                result.Add(new GenereDTO() { GenereId = movieGenere.GenereId, Name = movieGenere.Genere.Name });
            }
            return result;
        }
        private List<ActorDTO> MapMoviesActors(Movie movie, MovieDetailsDTO movieDetailsDTO)
        {
            var result = new List<ActorDTO>();
            foreach (var movieActor in movie.MoviesActors)
            {
                result.Add(new ActorDTO() { PersonId = movieActor.PersonId, Character = movieActor.Character, PersonName = movieActor.Person.Name });
            }
            return result;
        }
    }
}
