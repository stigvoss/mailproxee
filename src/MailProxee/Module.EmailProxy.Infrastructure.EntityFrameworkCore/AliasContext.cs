﻿using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Module.EmailProxy.Domain;
using System;
using System.Collections.Generic;
using System.Text;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Module.EmailProxy.Infrastructure.EntityFrameworkCore
{
    public class AliasContext : DbContext
    {
        public AliasContext()
        { }

        public AliasContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ForNpgsqlUseIdentityColumns();

            modelBuilder.Entity<Alias>()
                .HasKey(e => e.Id);

            modelBuilder.Entity<Alias>()
                .Property(e => e.Recipient)
                .IsRequired();

            modelBuilder.Entity<Alias>()
                .OwnsOne(e => e.ActivationCriteria);
        }

        public DbSet<Alias> Aliases { get; set; }
    }
}
