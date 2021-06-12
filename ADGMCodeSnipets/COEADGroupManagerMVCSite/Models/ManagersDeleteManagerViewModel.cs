using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManagerSQL;

namespace COEADGroupManager.Models
{
    public class ManagersDeleteManagerViewModel
    {
        public string dmStatus { get; set; }

        [Required(ErrorMessage = "User Name is required")]
        [RegularExpression("[a-zA-Z0-9\\-_\\.\\@]*", ErrorMessage = "User Name: invalid character entered")]
        public string delMgrID { get; set; }

        public AGMManager agmManager { get; set; }

        public ManagersDeleteManagerViewModel()
        {

        }
    }
}