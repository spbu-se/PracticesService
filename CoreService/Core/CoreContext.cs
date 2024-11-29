// <copyright file="CoreContext.cs" company="Gleb Kargin">
// Copyright (c) Gleb Kargin. All rights reserved.
// </copyright>

namespace CoreService.Core;

using System;
using System.Collections.Generic;
using CoreService.Core.Models;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Core service db context.
/// </summary>
public partial class CoreContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoreContext"/> class.
    /// </summary>
    public CoreContext()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CoreContext"/> class.
    /// </summary>
    /// <param name="options">Db context options.</param>
    public CoreContext(DbContextOptions<CoreContext> options)
        : base(options)
    {
    }

    /// <summary>
    /// Gets or sets DbSet Groups.
    /// </summary>
    public virtual DbSet<Group> Groups { get; set; }

    /// <summary>
    /// Gets or sets DbSet Lecturers.
    /// </summary>
    public virtual DbSet<Lecturer> Lecturers { get; set; }

    /// <summary>
    /// Gets or sets DbSet Practices.
    /// </summary>
    public virtual DbSet<Practice> Practices { get; set; }

    /// <summary>
    /// Gets or sets DbSet Students.
    /// </summary>
    public virtual DbSet<Student> Students { get; set; }

    /// <summary>
    /// Gets or sets DbSet Themes.
    /// </summary>
    public virtual DbSet<Theme> Themes { get; set; }

    /// <summary>
    /// On configuring method.
    /// </summary>
    /// <param name="optionsBuilder">Db context options builder.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost:5435;Database=postgres;Username=postgres;Password=postgres");
    }

    /// <summary>
    /// On model creating method.
    /// </summary>
    /// <param name="modelBuilder">Model builder.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("groups_pkey");

            entity.ToTable("groups");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
            entity.Property(e => e.Program)
                .HasMaxLength(500)
                .HasColumnName("program");
            entity.Property(e => e.Year).HasColumnName("year");
        });

        modelBuilder.Entity<Lecturer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lecturers_pkey");

            entity.ToTable("lecturers");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cansupervisevkr)
                .HasDefaultValue(false)
                .HasColumnName("cansupervisevkr");
            entity.Property(e => e.Department)
                .HasMaxLength(255)
                .HasColumnName("department");
            entity.Property(e => e.Userid).HasColumnName("userid");
        });

        modelBuilder.Entity<Practice>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("practices_pkey");

            entity.ToTable("practices");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createddate");
            entity.Property(e => e.Finalgrade)
                .HasMaxLength(5)
                .HasColumnName("finalgrade");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Studentid).HasColumnName("studentid");
            entity.Property(e => e.Themeid).HasColumnName("themeid");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Student).WithMany(p => p.Practices)
                .HasForeignKey(d => d.Studentid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("student_fk");

            entity.HasOne(d => d.Theme).WithMany(p => p.Practices)
                .HasForeignKey(d => d.Themeid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("theme_fk");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("students_pkey");

            entity.ToTable("students");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Groupid).HasColumnName("groupid");
            entity.Property(e => e.Userid).HasColumnName("userid");

            entity.HasOne(d => d.Group).WithMany(p => p.Students)
                .HasForeignKey(d => d.Groupid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("group_fk");
        });

        modelBuilder.Entity<Theme>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("themes_pkey");

            entity.ToTable("themes");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Consultantcontact)
                .HasMaxLength(255)
                .HasColumnName("consultantcontact");
            entity.Property(e => e.Consultantname)
                .HasMaxLength(255)
                .HasColumnName("consultantname");
            entity.Property(e => e.Createddate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("createddate");
            entity.Property(e => e.Department)
                .HasMaxLength(500)
                .HasColumnName("department");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Isarchived)
                .HasDefaultValue(false)
                .HasColumnName("isarchived");
            entity.Property(e => e.Suggestedby)
                .HasMaxLength(255)
                .HasColumnName("suggestedby");
            entity.Property(e => e.Supervisorid).HasColumnName("supervisorid");
            entity.Property(e => e.Tags)
                .HasColumnType("jsonb")
                .HasColumnName("tags");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .HasColumnName("title");
            entity.Property(e => e.Updateddate)
                .HasDefaultValueSql("now()")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updateddate");

            entity.HasOne(d => d.Supervisor).WithMany(p => p.Themes)
                .HasForeignKey(d => d.Supervisorid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("lecturer_fk");
        });

        this.OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
