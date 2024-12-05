using CMS_Project.Models.DTOs;


namespace CMS_Project.Services
{
    public interface IDocumentService
    {
        Task<List<DocumentDto>> GetAllDocumentsAsync(int userId);
        Task<DocumentResponseDto> GetDocumentByIdAsync(int documentId, int userId);
        Task<DocumentResponseDto> CreateDocumentAsync(DocumentCreateDto documentCreateDto, int userId);
        
        Task<bool> DeleteDocumentAsync(int id, int userId);
        Task<bool> UpdateDocumentAsync(int id, UpdateDocumentDto updateDocumentDto, int userId);

    }
}