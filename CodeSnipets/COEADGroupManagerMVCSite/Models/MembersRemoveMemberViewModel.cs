using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace COEADGroupManager.Models
{
    public class MembersRemoveMemberViewModel
    {
        public string rmStatus { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression("[a-zA-Z0-9\\-_\\.\\@]*", ErrorMessage = "User Name: invalid character entered")]
        public string rmvUserID { get; set; }

        public string rmvGrpID { get; set; }

        public MembersRemoveMemberViewModel()
        {

        }
    }
}