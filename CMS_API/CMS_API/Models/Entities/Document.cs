using System.ComponentModel.DataAnnotations;

namespace CMS_Project.Models.Entities
{
    public class Document
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;
        
        [Required]
        public string Content { get; set; } = string.Empty;
        
        [Required]
        public string ContentType { get; set; }  = string.Empty;

        public DateTime CreatedDate { get; set; }

        // Fremmednøkkel User
        [Required]
        public int UserId { get; set; }

        public User User { get; set; } = null!;
        
        // Fremmednøkkel Folder
        public int? FolderId { get; set; }

        public Folder? Folder { get; set; }
    }
}
