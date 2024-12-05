using System.ComponentModel.DataAnnotations;

namespace CMS_Project.Models.DTOs
{
    public class DocumentDto
    {
        [Required]
        public int DocumentId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [Required]
        public string ContentType { get; set; } = string.Empty;
        
        public DateTime CreatedDate { get; set; }
    }
}