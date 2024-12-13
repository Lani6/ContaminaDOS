using Microsoft.EntityFrameworkCore;
using ProyectoRedes.DataLayer.Models;
using System.Collections.Generic;

namespace ProyectoRedes.DataLayer.Context
{
    public class GameDbContext : DbContext
    {
        public GameDbContext(DbContextOptions<GameDbContext> options) : base(options) { }

        public DbSet<Game> Games { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Enemy> Enemies { get; set; }
        public DbSet<Round> Rounds { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Vote> Votes { get; set; }
        public DbSet<Actions> Actions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
  
            modelBuilder.Entity<Game>()
                .ToTable("games")
                .Property(g => g.Id).HasColumnName("id");
            modelBuilder.Entity<Game>()
                .Property(g => g.Name).HasColumnName("name");
            modelBuilder.Entity<Game>()
                .Property(g => g.Password).HasColumnName("password");
            modelBuilder.Entity<Game>()
                .Property(g => g.Status).HasColumnName("status");
            modelBuilder.Entity<Game>()
                .Property(g => g.CurrentRound).HasColumnName("current_round");

            modelBuilder.Entity<Player>()
                .ToTable("players")
                .Property(p => p.GameId).HasColumnName("game_id");
            modelBuilder.Entity<Player>()
                .Property(p => p.PlayerName).HasColumnName("player_name");

            modelBuilder.Entity<Enemy>()
                .ToTable("enemies")
                .Property(e => e.GameId).HasColumnName("game_id");
            modelBuilder.Entity<Enemy>()
                .Property(e => e.EnemyName).HasColumnName("enemy_name");

            modelBuilder.Entity<Round>()
                   .ToTable("rounds")
                   .Property(r => r.Id) 
                   .HasColumnName("id")
                   .IsRequired();

            modelBuilder.Entity<Round>()
                .Property(r => r.GameId)
                .HasColumnName("game_id")
                .IsRequired();

            modelBuilder.Entity<Round>()
                .Property(r => r.Leader)
                .HasColumnName("leader")
                .IsRequired();

            modelBuilder.Entity<Round>()
                .Property(r => r.Status)
                .HasColumnName("status")
                .HasDefaultValue("waiting-on-leader");

            modelBuilder.Entity<Round>()
                .Property(r => r.Result)
                .HasColumnName("result")
                .HasDefaultValue("none");

            modelBuilder.Entity<Round>()
                .Property(r => r.Phase)
                .HasColumnName("phase")
                .HasDefaultValue("vote1");

            modelBuilder.Entity<Round>()
                .Property(r => r.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("GETDATE()");


            modelBuilder.Entity<Group>()
              .ToTable("groups")
              .Property(g => g.Id).HasColumnName("id").IsRequired();
            modelBuilder.Entity<Group>()
                .Property(g => g.RoundId).HasColumnName("round_id").IsRequired();
            modelBuilder.Entity<Group>()
                .Property(g => g.PlayerName).HasColumnName("player_name").IsRequired();
            modelBuilder.Entity<Group>()
                .Property(g => g.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<Vote>()
               .ToTable("votes")
               .Property(v => v.Id).HasColumnName("id").IsRequired();

            modelBuilder.Entity<Vote>()
                .Property(v => v.RoundId).HasColumnName("round_id").IsRequired();

            modelBuilder.Entity<Vote>()
                .Property(v => v.PlayerName).HasColumnName("player_name").IsRequired();

            modelBuilder.Entity<Vote>()
                .Property(v => v.VoteValue).HasColumnName("vote").IsRequired();


            modelBuilder.Entity<Actions>()
                .ToTable("actions")
                .Property(a => a.Id).HasColumnName("id").IsRequired();
            modelBuilder.Entity<Actions>()
                .Property(a => a.RoundId).HasColumnName("round_id").IsRequired();
            modelBuilder.Entity<Actions>()
                .Property(a => a.PlayerName).HasColumnName("player_name").IsRequired();
            modelBuilder.Entity<Actions>()
                .Property(a => a.ActionValue).HasColumnName("action").IsRequired();
            modelBuilder.Entity<Actions>()
                .Property(a => a.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("GETDATE()");

        }
    }
}