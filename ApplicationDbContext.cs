using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebAPITutorial.Entities;

namespace WebAPITutorial
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext([NotNullAttribute] DbContextOptions options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MoviesGeneres>().HasKey(g => new { g.GenereId, g.MovieId });
            modelBuilder.Entity<MoviesActors>().HasKey(a => new { a.PersonId, a.MovieId });
            base.OnModelCreating(modelBuilder);
        }
        public DbSet<Genere> Generes { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public MoviesGeneres MoviesGeneres { get; set; }
        public MoviesActors MoviesActors { get; set; }
    }
}
