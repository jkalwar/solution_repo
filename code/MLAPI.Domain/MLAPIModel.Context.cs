﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace MLAPI.Domain
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class MLAPIEntities : DbContext
    {
        public MLAPIEntities()
            : base("name=MLAPIEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Experiment> Experiments { get; set; }
        public virtual DbSet<Image> Images { get; set; }
        public virtual DbSet<Model> Models { get; set; }
        public virtual DbSet<AccuracyParamter> AccuracyParamters { get; set; }
    }
}
