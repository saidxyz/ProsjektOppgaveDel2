using System.ComponentModel.DataAnnotations;

namespace CMS_Project.Models.DTOs
{
    public class LoginDto
    {   
        [Required]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
