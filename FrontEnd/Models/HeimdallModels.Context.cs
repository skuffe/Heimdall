﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FrontEnd.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class heimdallEntities : DbContext
    {
        public heimdallEntities()
            : base("name=heimdallEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<tbl_ClientInfo> tbl_ClientInfo { get; set; }
        public DbSet<tbl_Clients> tbl_Clients { get; set; }
        public DbSet<tbl_ClientTypes> tbl_ClientTypes { get; set; }
        public DbSet<tbl_Groups> tbl_Groups { get; set; }
        public DbSet<tbl_InterfaceInfo> tbl_InterfaceInfo { get; set; }
        public DbSet<tbl_Interfaces> tbl_Interfaces { get; set; }
        public DbSet<tbl_Processes> tbl_Processes { get; set; }
        public DbSet<tbl_ProcessInfo> tbl_ProcessInfo { get; set; }
    }
}
