using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Navigate.Models
{
    [Table("UserProfiles")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }

        [MaxLength(120)]
        public string UserName { get; set; }

        [MaxLength(254)]
        public string Email { get; set; }

        public string BaseLocation { get; set; }
    }
}