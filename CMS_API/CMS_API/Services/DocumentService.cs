using CMS_Project.Data;
using CMS_Project.Models.Entities;
using CMS_Project.Models.DTOs;
using Microsoft.EntityFrameworkCore;


namespace CMS_Project.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly CMSContext _context;

        public DocumentService(CMSContext context)
        {
            _context = context;
        }

        /// Retrieves all documents for a given user ID.
        public async Task<List<DocumentDto>> GetAllDocumentsAsync(int userId)
        {
            var documents = await _context.Documents
                .Where(d => d.UserId == userId)
                .AsNoTracking()
                .Select(d => new DocumentDto
                {
                    DocumentId = d.Id,
                    Title = d.Title,
                    Content = d.Content,
                    ContentType = d.ContentType,
                    CreatedDate = d.CreatedDate
                })
                .ToListAsync();

            return documents;
        }
        
        /// Retrieves a document by its ID for a specified user, including its folder details.
        public async Task<DocumentResponseDto> GetDocumentByIdAsync(int documentId, int userId)
        {
            var document = await _context.Documents
                .Include(d => d.Folder)
                .FirstOrDefaultAsync(d => d.Id == documentId && d.UserId == userId);

            if (document == null)
            {
                throw new KeyNotFoundException("Document not found or does not belong to the user.");
            }
    
            FolderDto? folderDto = null;
            
            if (document.Folder != null)
            {
                folderDto = new FolderDto
                {
                    FolderId = document.Folder.Id,
                    Name = document.Folder.Name,
                    CreatedDate = document.Folder.CreatedDate,
                    ParentFolderId = document.Folder.ParentFolderId,
                    ChildrenFolders = new List<FolderDto>()
                };
            }

            var documentDetailDto = new DocumentDetailDto
            {
                DocumentId = document.Id,
                Title = document.Title,
                Content = document.Content,
                ContentType = document.ContentType,
                CreatedDate = document.CreatedDate,
                FolderId = document.FolderId ?? 0
            };
            var responseDto = new DocumentResponseDto
            {
                Folder = folderDto,
                Document = documentDetailDto
            };
    
            return responseDto;
        }

        /// Creates a new document for a specified user and validates ownership of the folder.
        public async Task<DocumentResponseDto> CreateDocumentAsync(DocumentCreateDto documentCreateDto, int userId)
        {
            int? folderId = documentCreateDto.FolderId;

            // Optional check for FolderId: If null, assign a default or handle as necessary
            if (folderId.HasValue)
            {
                var folder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);

                if (folder == null)
                {
                    throw new ArgumentException("Folder not found or does not belong to the user.");
                }
            }
            else
            {
                // Optional: Assign a default folder or handle the absence of a folder as needed.
                folderId = null; // or some default folder ID if applicable
            }

            var document = new Document
            {
                Title = documentCreateDto.Title,
                Content = documentCreateDto.Content,
                ContentType = documentCreateDto.ContentType,
                CreatedDate = DateTime.UtcNow,
                UserId = userId,
                FolderId = folderId
            };

            _context.Documents.Add(document);
            await _context.SaveChangesAsync();

            return await GetDocumentByIdAsync(document.Id, userId);
        }
        
        /// Updates a document by its ID for a specified user.
        public async Task<bool> UpdateDocumentAsync(int id, UpdateDocumentDto updateDocumentDto, int userId)
        {
            // Find the document
            var document = await _context.Documents.FindAsync(id);
            if (document == null)
                return false;

            // Check if the user owns the document
            if (document.UserId != userId)
                throw new UnauthorizedAccessException("User doesn't have access to this document.");

            // Only check folder ownership if a valid folderId is provided (greater than 0)
            if (updateDocumentDto.FolderId > 0)
            {
                var folder = await _context.Folders.FindAsync(updateDocumentDto.FolderId);
                if (folder == null || folder.UserId != userId)
                    throw new UnauthorizedAccessException("User doesn't have access to this folder.");
            }

            // Update document properties
            document.Title = updateDocumentDto.Title;
            document.Content = updateDocumentDto.Content;
            document.ContentType = updateDocumentDto.ContentType;
            document.FolderId = updateDocumentDto.FolderId > 0 ? updateDocumentDto.FolderId : null; // Set to null if folderId is 0

            // Mark document as modified
            _context.Entry(document).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await DocumentExists(id))
                    return false;

                throw;
            }

            return true;
        }
        
        /// Deletes a document by its ID for a specified user.
        public async Task<bool> DeleteDocumentAsync(int id, int userId)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document == null || document.UserId != userId)
                return false;

            _context.Documents.Remove(document);
            await _context.SaveChangesAsync();
            return true;
        }
        
        /// <summary>
        /// GET document by id given
        public async Task<Document> GetDocumentByIdAsync(int id)
        {
            return await _context.Documents
                .Include(d => d.User)
                .Include(d => d.Folder)
                .FirstOrDefaultAsync(d => d.Id == id);
        }
        
        /// Checks if a document exists by its ID.
        private async Task<bool> DocumentExists(int id)
        {
            return await _context.Documents.AnyAsync(e => e.Id == id);
        }
    }
}
