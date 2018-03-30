using Microsoft.ProjectOxford.Face.Contract;
using System;
using System.Threading.Tasks;
using System.IO;

namespace ImageGallery.Services
{
    public interface ICognitiveService
    {
        Task<Face[]> UploadAndDetectFaces(Uri imageUri);
    }
}
