//------------------------------------------------------------------------------
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
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    
    public partial class tbl_Clients
    {
        public tbl_Clients()
        {
            this.tbl_ClientInfo = new HashSet<tbl_ClientInfo>();
            this.tbl_Interfaces = new HashSet<tbl_Interfaces>();
            this.tbl_Processes = new HashSet<tbl_Processes>();
        }
    
        [Key]
        public int ClientID { get; set; }
        public Nullable<int> GroupID { get; set; }
        public Nullable<int> ClientTypeID { get; set; }
        public string HostName { get; set; }
        public string IPAddress { get; set; }
    
        public virtual ICollection<tbl_ClientInfo> tbl_ClientInfo { get; set; }
        public virtual tbl_ClientTypes tbl_ClientTypes { get; set; }
        public virtual tbl_Groups tbl_Groups { get; set; }
        public virtual ICollection<tbl_Interfaces> tbl_Interfaces { get; set; }
        public virtual ICollection<tbl_Processes> tbl_Processes { get; set; }
    }
}
