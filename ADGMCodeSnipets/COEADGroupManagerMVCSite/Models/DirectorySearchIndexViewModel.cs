using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using COEADGroupManager.Models;

namespace COEADGroupManager.Models
{
    public class DirectorySearchIndexViewModel
    {
        public string dsiStatus { get; set; }

        [Required(ErrorMessage = "Please Type in a Search Term")]
        [RegularExpression("[a-zA-Z0-9\\-_\\.\\@\\'\\s]*", ErrorMessage = "Invalid Format")]
        public string uSearchTerm { get; set; }

        public ADGMUsrSearchADResultCollection rsltCollection { get; set; }

        public DirectorySearchIndexViewModel()
        {

        }

    }
}