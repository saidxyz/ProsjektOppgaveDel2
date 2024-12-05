using CMS_Project.Data;
using CMS_Project.Models.Entities;
using CMS_Project.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace CMS_Project.Services
{
    public class FolderService : IFolderService
    {
        private readonly CMSContext _context;
        private readonly ILogger<FolderService> _logger; 
        private IFolderService _folderServiceImplementation;

        public FolderService(CMSContext context,  ILogger<FolderService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        /// Retrieves a folder by its ID, including documents and child folders.
        public async Task<Folder> GetFolderByIdAsync(int id)
        {
            return await _context.Folders
                .Include(f => f.Documents)
                .Include(f => f.ChildrenFolders)
                .FirstOrDefaultAsync(f => f.Id == id);
        }
        
        /// Gets all folders for a specific user.
        public async Task<IEnumerable<Folder>> GetAllFoldersAsync(int userId)
        {
            return await _context.Folders
                .Include(f => f.Documents)
                .Include(f => f.User)
                .Where(f => f.UserId == userId)
                .ToListAsync();
        }

        /// Retrieves all top-level folders (those without a parent folder) for a specific user.
        public async Task<IEnumerable<Folder>> GetFoldersByUserIdAsync(int userId)
        {
            return await _context.Folders
                .Where(f => f.UserId == userId && f.ParentFolderId == null)
                .Include(f => f.ChildrenFolders)
                .ToListAsync();
        }

        /// Retrieves a folder by ID for a specific user with detailed information.
        public async Task<FolderDetailDto> GetFolderByIdAsync(int folderId, int userId)
        {
            var folder = await _context.Folders
                .Include(f => f.ChildrenFolders)
                .Include(f => f.Documents)
                .FirstOrDefaultAsync(f => f.Id == folderId && f.UserId == userId);

            if (folder == null)
            {
                throw new KeyNotFoundException("Folder not found or does not belong to the user.");
            }

            var folderDetailDto = MapToFolderDetailDto(folder);
            return folderDetailDto;
        }

        /// GET all folders where UserId = Documents-User.Id
        public async Task<List<FolderDto>> GetAllFoldersAsDtoAsync(int userId)
        {
            var rootFolders = await _context.Folders
                .Where(f => f.UserId == userId && f.ParentFolderId == null)
                .ToListAsync();

            var folderDtos = rootFolders.Select(folder => MapToFolderDtoRecursively(folder)).ToList();
            return folderDtos;
        }

        /// Recursively maps a folder and its children to FolderDto.
        private FolderDto MapToFolderDtoRecursively(Folder folder)
        {
            // Map basic properties of the folder
            var folderDto = new FolderDto
            {
                FolderId = folder.Id,
                Name = folder.Name,
                CreatedDate = folder.CreatedDate,
                ParentFolderId = folder.ParentFolderId,
                ChildrenFolders = new List<FolderDto>()
            };

            // Retrieve children folders and map them recursively
            var children = _context.Folders
                .Where(f => f.ParentFolderId == folder.Id)
                .ToList(); // Execute query here to avoid EF tracking issues in recursion

            foreach (var child in children)
            {
                folderDto.ChildrenFolders.Add(MapToFolderDtoRecursively(child));
            }

            return folderDto;
        }

        /// CREATE folder by Dto and checks ownership. First folder needs parentFolderId to be null!!
        public async Task CreateFolderAsync(Folder folder)
        {
            if (folder.ParentFolderId.HasValue)
            {
                var parentFolder = await _context.Folders
                    .FirstOrDefaultAsync(f => f.Id == folder.ParentFolderId && f.UserId == folder.UserId);
                
                if (parentFolder == null)
                {
                    throw new ArgumentException("Parent folder not found or does not belong to the user.");
                }
            }
            _context.Folders.Add(folder);
            await _context.SaveChangesAsync();
        }
        
        /// Updates a folder by its ID for a specific user.
        public async Task<bool> UpdateFolderAsync(int id, UpdateFolderDto updateFolderDto, int userId)
        {
            var folder = await _context.Folders.FindAsync(id);
            if (folder == null)
                throw new ArgumentException("folder not found.");
            
            if (folder.UserId != userId)
                throw new ArgumentException("User doesn't own folder.");

            if (updateFolderDto.ParentFolderId != null)
            {
                //check if user owns parent folder.
                var parentfolder = await _context.Folders.FirstAsync(f => f.Id == updateFolderDto.ParentFolderId);
                if (folder.ParentFolderId == null)
                    if (parentfolder.UserId != userId)
                        throw new ArgumentException("User doesn't own parent folder.");
            }

            folder.Name = updateFolderDto.Name;
            folder.ParentFolderId = updateFolderDto.ParentFolderId;

            _context.Entry(folder).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await FolderExists(id))
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }

            return true;
        }
        
        /// Deletes a folder by its ID for a specific user, including all child folders and documents.
        public async Task<bool> DeleteFolderAsync(int id, int userId)
        {
            try
            {
                var folder = await _context.Folders
                    .Include(f => f.ChildrenFolders)
                    .Include(f => f.Documents)
                    .FirstOrDefaultAsync(f => f.Id == id && f.UserId == userId);
        
                if (folder == null)
                {
                    return false;
                }

                // Delete all subfolders and documents recursively
                DeleteFolderRecursive(folder);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception (you can use your logger here)
                _logger.LogError(ex, $"Error deleting folder with ID {id} for user {userId}");
                throw; // Re-throw the exception to be handled by the controller
            }
        }
        
        // Helper Methods

        /// Recursively deletes a folder and all its child folders and documents.
        private void DeleteFolderRecursive(Folder folder)
        {
            // Delete all documents in folder
            _context.Documents.RemoveRange(folder.Documents);
    
            // Recursively delete all child folders
            foreach (var childFolder in folder.ChildrenFolders)
            {
                // Load child folder's children and documents
                _context.Entry(childFolder).Collection(f => f.ChildrenFolders).Load();
                _context.Entry(childFolder).Collection(f => f.Documents).Load();
        
                // Recursive call
                DeleteFolderRecursive(childFolder);
            }
    
            // Delete the current folder
            _context.Folders.Remove(folder);
        }

        /// Checks if a folder with the specified ID exists.
        private async Task<bool> FolderExists(int id)
        {
            return await _context.Folders.AnyAsync(f => f.Id == id);
        }
        
        /// Maps a Folder entity to a FolderDetailDto.
        private FolderDetailDto MapToFolderDetailDto(Folder folder)
        {
            return new FolderDetailDto
            {
                FolderId = folder.Id,
                Name = folder.Name,
                CreatedDate = folder.CreatedDate,
                ParentFolderId = folder.ParentFolderId,
                Documents = folder.Documents.Select(d => new DocumentDto
                {
                    DocumentId = d.Id,
                    Title = d.Title,
                    Content = d.Content,
                    ContentType = d.ContentType,
                    CreatedDate = d.CreatedDate
                }).ToList(),
                ChildrenFolders = folder.ChildrenFolders.Select(MapToFolderDto).ToList()
            };
        }

        /// Maps a Folder entity to a FolderDto.
        private FolderDto MapToFolderDto(Folder folder)
        {
            return new FolderDto
            {
                FolderId = folder.Id,
                Name = folder.Name,
                CreatedDate = folder.CreatedDate,
                ParentFolderId = folder.ParentFolderId,
                ChildrenFolders = folder.ChildrenFolders?.Select(MapToFolderDto).ToList() ?? new List<FolderDto>()
            };
        }
        
    }
}
