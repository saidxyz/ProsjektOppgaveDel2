using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Collections.Generic;

public class CustomDocumentOrderFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        //Auth first, then Folder, then Document
        var orderedPaths = swaggerDoc.Paths
            .OrderBy(path => 
                path.Key.Contains("/api/Auth") ? 0 :
                path.Key.Contains("/api/Folder") ? 1 :
                path.Key.Contains("/api/Document") ? 2 : 
                3)  // Orders Auth first, Folder second, Document third, other endpoints last
            .ToDictionary(entry => entry.Key, entry => entry.Value);

        // Reassign sorted paths to swaggerDoc
        swaggerDoc.Paths = new OpenApiPaths();
        foreach (var path in orderedPaths)
        {
            swaggerDoc.Paths.Add(path.Key, path.Value);
        }
    }
}