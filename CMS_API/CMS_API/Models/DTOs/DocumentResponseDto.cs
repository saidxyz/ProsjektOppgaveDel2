namespace CMS_Project.Models.DTOs
{
    public class DocumentResponseDto
    {
        public FolderDto? Folder { get; set; }
        public DocumentDetailDto Document { get; set; } = new DocumentDetailDto();
    }
    
}