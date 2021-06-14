using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{

    public class ManagersGroupAddViewModel
    {

        public string gaStatus { get; set; }

        public string addGrpID { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression("[a-zA-Z0-9\\-_\\.\\@]*", ErrorMessage = "User Name: invalid character entered")]
        public string addMgrID { get; set; }

        public ManagersGroupAddViewModel()
        {

        }

    }
}