//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace COEADGroupManagerSQL
{
    using System;
    using System.Collections.Generic;
    
    public partial class AGMManagerDisaffiliateRqst
    {
        public int AMDRID { get; set; }
        public string uDept { get; set; }
        public string KerbID { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public string SubmittedBy { get; set; }
        public Nullable<System.DateTime> SubmittedOn { get; set; }
        public Nullable<System.DateTime> CompletedOn { get; set; }
        public Nullable<System.DateTime> DisaffiliateOn { get; set; }
        public Nullable<bool> uPending { get; set; }
        public Nullable<bool> uCancelled { get; set; }
    }
}