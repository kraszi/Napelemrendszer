﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Napelemrendszer.Model.EF
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class localNapelemEntities : DbContext
    {
        public localNapelemEntities()
            : base("name=localNapelemEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<C__EFMigrationsHistory> C__EFMigrationsHistory { get; set; }
        public virtual DbSet<Component> Components { get; set; }
        public virtual DbSet<ComponentsToProject> ComponentsToProjects { get; set; }
        public virtual DbSet<Employee> Employees { get; set; }
        public virtual DbSet<Progress> Progresses { get; set; }
        public virtual DbSet<Projet> Projets { get; set; }
        public virtual DbSet<Storage> Storages { get; set; }
    }
}