using CMS_Project.Models.Entities;
using CMS_Project.Models.DTOs;

namespace CMS_Project.Services
{
    public interface IFolderService
    {
        Task CreateFolderAsync(Folder folder);
        Task<List<FolderDto>> GetAllFoldersAsDtoAsync(int userId);
        Task<FolderDetailDto> GetFolderByIdAsync(int folderId, int userId);
        Task<bool> UpdateFolderAsync(int id, UpdateFolderDto updateFolderDto, int userId);
        Task<bool> DeleteFolderAsync(int id, int userId);
    }
}