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
    
    public partial class AGMManager
    {
        public AGMManager()
        {
            this.AGMGroups = new HashSet<AGMGroup>();
        }
    
        public int AGMMID { get; set; }
        public string KerbID { get; set; }
        public string DisplayName { get; set; }
        public string EmailAddress { get; set; }
        public Nullable<bool> SendAllGrpsRpt { get; set; }
        public Nullable<int> DaysBtwnReport { get; set; }
        public Nullable<System.DateTime> RptLastSent { get; set; }
    
        public virtual ICollection<AGMGroup> AGMGroups { get; set; }
    }
}