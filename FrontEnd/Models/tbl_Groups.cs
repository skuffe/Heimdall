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
    
    public partial class tbl_Groups
    {
        public tbl_Groups()
        {
            this.tbl_Clients = new HashSet<tbl_Clients>();
        }
    
        public int GroupID { get; set; }
        public string GroupName { get; set; }
    
        public virtual ICollection<tbl_Clients> tbl_Clients { get; set; }
    }
}