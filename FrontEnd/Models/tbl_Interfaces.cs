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
    
    public partial class tbl_Interfaces
    {
        public tbl_Interfaces()
        {
            this.tbl_InterfaceInfo = new HashSet<tbl_InterfaceInfo>();
        }
    
        public int InterfaceID { get; set; }
        [Required]
        public int ClientID { get; set; }
        [Required]
        public string InterfaceName { get; set; }
    
        public virtual tbl_Clients tbl_Clients { get; set; }
        public virtual ICollection<tbl_InterfaceInfo> tbl_InterfaceInfo { get; set; }
    }
}
