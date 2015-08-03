//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DataAccess.MSSQL
{
    using System;
    using System.Collections.Generic;
    
    public partial class User
    {
        public User()
        {
            this.Farms = new HashSet<Farm>();
            this.ScoutStops = new HashSet<ScoutStop>();
            this.UserRoles = new HashSet<UserRole>();
        }
    
        public int UserID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public Nullable<int> LastModifiedID { get; set; }
        public Nullable<System.DateTime> TMStamp { get; set; }
    
        public virtual ICollection<Farm> Farms { get; set; }
        public virtual ICollection<ScoutStop> ScoutStops { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
