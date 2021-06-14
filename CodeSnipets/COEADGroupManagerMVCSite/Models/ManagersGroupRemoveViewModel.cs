using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class ManagersGroupRemoveViewModel
    {
        public string grStatus { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression("[a-zA-Z0-9\\-_\\.\\@]*", ErrorMessage = "User Name: invalid character entered")]
        public string rmvMgrID { get; set; }

        public string rmvGrpID { get; set; }

        public ManagersGroupRemoveViewModel()
        {

        }

    }
}