using ImageGallery.Services;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.ProjectOxford.Face.Contract;
using System.Runtime.Serialization.Formatters.Binary;

namespace ImageGallery.Controllers
{
    public class HomeController : Controller
    {
        private readonly IStorageService _storageService;
        private readonly ICognitiveService _cognitiveService;

        public HomeController(IStorageService storageService, ICognitiveService cognitiveService)
        {
            _storageService = storageService;
            _cognitiveService = cognitiveService;
        }

        public async Task<ActionResult> Index()
        {
            var images = await _storageService.GetImagesAsync();

            // draw boxes
            foreach (var image in images)
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(image.ImagePath);
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                Bitmap img = new Bitmap(responseStream);

                using (Graphics g = Graphics.FromImage(img))
                {
                    if (image.FaceAttributes.Count > 0)
                    {
                        foreach (Face face in image.FaceAttributes)
                        {
                            g.DrawRectangle(new Pen(Brushes.Red, 2), new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height));
                        }
                    }
                }
                img.Save(@"C:\Temp\img" + image.ImageGuid + ".png");
            }

            // face swap
            foreach (var image in images)
            {
                System.Net.WebRequest request = System.Net.WebRequest.Create(image.ImagePath);
                System.Net.WebResponse response = request.GetResponse();
                System.IO.Stream responseStream = response.GetResponseStream();
                Bitmap srcBitmap = new Bitmap(responseStream);
                Bitmap destBitmap = DeepCopy(srcBitmap);

                using (Graphics g = Graphics.FromImage(destBitmap))
                {
                    if (image.FaceAttributes.Count > 1)
                    {
                        Rectangle[] rect = new Rectangle[image.FaceAttributes.Count];
                        int i = 0;
                        foreach (Face face in image.FaceAttributes)
                        {
                            rect[i] = new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height);
                            i++;
                        }
                        i = 0;
                        foreach (Face face in image.FaceAttributes)
                        {
                            if (i != 0)
                            {
                                g.DrawImage(srcBitmap, new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height), rect[i - 1], GraphicsUnit.Pixel);
                            }
                            else
                            {
                                g.DrawImage(srcBitmap, new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height), rect[rect.Length - 1], GraphicsUnit.Pixel);
                            }
                            i++;
                        }
                        destBitmap.Save(@"C:\Temp\imgSwap" + image.ImageGuid + ".png");
                    }
                }
            }

            // draw boxes
        //    foreach (var image in images)
        //    {
        //        System.Net.WebRequest request = System.Net.WebRequest.Create(image.ImagePath);
        //        System.Net.WebResponse response = request.GetResponse();
        //        System.IO.Stream responseStream = response.GetResponseStream();
        //        Bitmap img = new Bitmap(responseStream);

        //        using (Graphics g = Graphics.FromImage(img))
        //        {
        //            if (image.FaceAttributes.Count > 0)
        //            {
        //                foreach (Face face in image.FaceAttributes)
        //                {
        //                    g.DrawRectangle(new Pen(Brushes.Red, 2), new Rectangle(face.FaceRectangle.Left, face.FaceRectangle.Top, face.FaceRectangle.Width, face.FaceRectangle.Height));
        //                    g.
        //                }
        //            }
        //        }
        //        img.Save(@"C:\Temp\img" + image.ImageGuid + ".png");
        //    }

            return View(images);
        }

        public static T DeepCopy<T>(T other)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, other);
                ms.Position = 0;
                return (T)formatter.Deserialize(ms);
            }
        }

        [HttpPost]
        public async Task<ActionResult> Upload(HttpPostedFileBase file)
        {
            if (file == null)
            {
                return RedirectToAction("Index", new { value = "uploadFailure" });
            }

            try
            {
                var fileExtension = Path.GetExtension(file.FileName);


               
                var image = await _storageService.AddImageAsync(file.InputStream, fileExtension);
                var faces = await _cognitiveService.UploadAndDetectFaces(image.ImagePath);


                await _storageService.AddMetadataAsync(image, faces);
            }
            catch (Exception e)
            {
                return RedirectToAction("Index", new { value = "uploadFailure" });
            }
            
            return RedirectToAction("Index", new { value = "uploadSuccess" });
        }
    }
}