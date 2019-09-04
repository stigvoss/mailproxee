﻿using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Module.EmailProxy.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Module.EmailProxy.Infrastructure.EntityFrameworkCore
{
    public class AliasContext : DbContext
    {
        public AliasContext(DbContextOptions options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

        }

        public DbSet<Alias> Aliases { get; set; }
    }
}