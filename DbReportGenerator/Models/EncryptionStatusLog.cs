//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace DbReportGenerator.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class EncryptionStatusLog
    {
        public string Application { get; set; }
        public Nullable<int> LDF { get; set; }
        public Nullable<int> MDF { get; set; }
        public Nullable<int> NDF { get; set; }
        public Nullable<int> Encrypted { get; set; }
        public Nullable<int> Unencrypted_Non_Compliant { get; set; }
        public Nullable<int> TBD_ABRI_to_be_verified { get; set; }
        public Nullable<int> Not_in_scope_System_Database { get; set; }
        public Nullable<int> Not_in_scope_MMS_Corporate { get; set; }
        public Nullable<int> Not_in_scope_Delphix { get; set; }
        public Nullable<int> Not_in_scope_To_be_decommissioned { get; set; }
    }
}
