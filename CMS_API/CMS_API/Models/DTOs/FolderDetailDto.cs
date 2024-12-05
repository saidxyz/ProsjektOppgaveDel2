using System;
using System.Collections.Generic;

namespace CMS_Project.Models.DTOs
{
    public class FolderDetailDto
    {
        public int FolderId { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int? ParentFolderId { get; set; }

        public List<DocumentDto> Documents { get; set; } = new List<DocumentDto>();
        public List<FolderDto> ChildrenFolders { get; set; } = new List<FolderDto>();
    }
}