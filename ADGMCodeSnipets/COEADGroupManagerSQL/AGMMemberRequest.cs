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
    
    public partial class AGMMemberRequest
    {
        public AGMMemberRequest()
        {
            this.AGMGroups = new HashSet<AGMGroup>();
        }
    
        public int AGMMRID { get; set; }
        public string KerbID { get; set; }
        public string MRAction { get; set; }
        public string SubmittedBy { get; set; }
        public Nullable<System.DateTime> SubmittedOn { get; set; }
        public Nullable<System.DateTime> CompletedOn { get; set; }
        public Nullable<bool> Pending { get; set; }
        public string ADGroupName { get; set; }
    
        public virtual ICollection<AGMGroup> AGMGroups { get; set; }
    }
}
