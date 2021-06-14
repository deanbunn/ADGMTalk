using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class AddGroupIndexViewModel
    {
        
        [Required(ErrorMessage = "Please Type in a Search Term")]
        [RegularExpression("[a-zA-Z0-9\\-_\\.\\'\\s]*", ErrorMessage = "Invalid Format")]
        public string srchName { get; set; }

        [Required(ErrorMessage = "*")]
        [RegularExpression("[a-zA-Z0-9]*", ErrorMessage = "Invalid Format")]
        public string srchDomain { get; set; }

        public ADGMGrpSrchCollection adGrpCol { get; set; }

        public AddGroupIndexViewModel()
        {

        }

    }
}