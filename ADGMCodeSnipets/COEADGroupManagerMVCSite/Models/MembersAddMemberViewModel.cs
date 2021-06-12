using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class MembersAddMemberViewModel
    {
        public string amStatus { get; set; }

        public AGMGroup agmGroup { get; set; }

        [Required(ErrorMessage = "User IDs are required")]
        public string nUsers { get; set; }

        public List<string> lErrorMsgs { get; set; }

        public MembersAddMemberViewModel()
        {
            lErrorMsgs = new List<string>();
        }

    }
}