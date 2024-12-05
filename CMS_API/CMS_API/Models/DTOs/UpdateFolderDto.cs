using System.ComponentModel.DataAnnotations;

namespace CMS_Project.Models.DTOs
{
    public class UpdateFolderDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public int? ParentFolderId { get; set; }
    }
}