using System;
using System.Threading.Tasks;
using System.Web.Configuration;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using System.IO;

namespace ImageGallery.Services
{
    public class CognitiveService : ICognitiveService
    {
        private readonly IFaceServiceClient _client;

        public CognitiveService()
        {
            var url = WebConfigurationManager.AppSettings["CognitiveServiciesUrl"];
            var key = WebConfigurationManager.AppSettings["CognitiveServicesFaceApiKey"];

            _client = new FaceServiceClient(key, url);
        }

        public async Task<Face[]> UploadAndDetectFaces(Uri imageUri)
        {
            var attributes = new FaceAttributeType[]
            {
                FaceAttributeType.Gender,
                FaceAttributeType.Age,
                FaceAttributeType.Smile,
                FaceAttributeType.Emotion,
                FaceAttributeType.Glasses,
                FaceAttributeType.Hair
            };
            try
            {
                Face[] faces = await _client.DetectAsync(imageUri.ToString(), returnFaceId: true, returnFaceLandmarks: true, returnFaceAttributes: attributes);
                return faces;
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                Console.WriteLine(f.ErrorMessage + " " + f.ErrorCode);
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new Face[0];
            }
        }
    }
}