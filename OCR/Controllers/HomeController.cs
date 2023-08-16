using AForge.Imaging;
using AForge.Imaging.Filters;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using OCR.Models;
using System.Diagnostics;
using System.Drawing;
using Tesseract;
using static System.Net.Mime.MediaTypeNames;

namespace OCR.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(ILogger<HomeController> logger,IWebHostEnvironment webHostEnvironment)
        {
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public IActionResult Index()
        {

            return View();
        }


        public class ocrModel
        {
            public IFormFile pic { get; set; }
        }


        public IActionResult GrayscaleImage()
        {
            // Load the original image
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", "khaled-ID.jpg");

            Bitmap originalImage = new Bitmap(filePath);

            // Apply grayscale filter
            Grayscale gray = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayscaleImage = gray.Apply(originalImage);

            // Convert the grayscale image to a byte array
            using (MemoryStream ms = new MemoryStream())
            {
                grayscaleImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();

                // Dispose of the bitmap objects to release resources
                originalImage.Dispose();
                grayscaleImage.Dispose();

                // Return the grayscale image as a FileContentResult
                return new FileContentResult(imageBytes, "image/png");
            }
        }


        public IActionResult GrayscaleWithmEDImage()
        {
            // Load the original image
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", "khaled-ID.jpg");
            Bitmap originalImage = new Bitmap(filePath);

            // Apply grayscale filter
            Grayscale gray = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayscaleImage = gray.Apply(originalImage);

            // Apply median filter to remove noise
            Bitmap denoisedImage = ApplyMedianFilter(grayscaleImage);

            // Convert the denoised image to a byte array
            using (MemoryStream ms = new MemoryStream())
            {
                denoisedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();

                // Dispose of the bitmap objects to release resources
                originalImage.Dispose();
                grayscaleImage.Dispose();
                denoisedImage.Dispose();

                // Return the denoised image as a FileContentResult
                return new FileContentResult(imageBytes, "image/png");
            }
        }

        private Bitmap ApplyMedianFilter(Bitmap inputImage)
        {
            Bitmap denoisedImage = new Bitmap(inputImage.Width, inputImage.Height);

            for (int y = 1; y < inputImage.Height - 1; y++)
            {
                for (int x = 1; x < inputImage.Width - 1; x++)
                {
                    List<int> neighborPixels = new List<int>();

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            Color pixelColor = inputImage.GetPixel(x + j, y + i);
                            int averageColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;
                            neighborPixels.Add(averageColor);
                        }
                    }

                    neighborPixels.Sort();

                    int medianValue = neighborPixels[neighborPixels.Count / 2];
                    Color denoisedColor = Color.FromArgb(medianValue, medianValue, medianValue);

                    denoisedImage.SetPixel(x, y, denoisedColor);
                }
            }

            return denoisedImage;
        }

        public IActionResult GrayscaleWithTheadImage()
        {
            // Load the original image
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", "khaled-ID.jpg");
            Bitmap originalImage = new Bitmap(filePath);

            // Apply grayscale filter
            Grayscale gray = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayscaleImage = gray.Apply(originalImage);

            // Apply threshold
            int thresholdValue = 128; // Adjust this threshold value as needed
            Bitmap thresholdedImage = ApplyThreshold(grayscaleImage, thresholdValue);

            // Convert the thresholded image to a byte array
            using (MemoryStream ms = new MemoryStream())
            {
                thresholdedImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();

                // Dispose of the bitmap objects to release resources
                originalImage.Dispose();
                grayscaleImage.Dispose();
                thresholdedImage.Dispose();

                // Return the thresholded image as a FileContentResult
                return new FileContentResult(imageBytes, "image/png");
            }
        }

        public IActionResult EnhanceImage()
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", "khaled-ID.jpg");
            Bitmap originalImage = new Bitmap(filePath);

            // Resize the image
            int newWidth = originalImage.Width * 2; // Increase width by a factor of 2
            int newHeight = originalImage.Height * 2; // Increase height by a factor of 2
            Bitmap resizedImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(resizedImage).DrawImage(originalImage, new Rectangle(0, 0, newWidth, newHeight));

            // Apply filters
            Grayscale filter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayscaleImage = filter.Apply(resizedImage);

            ContrastStretch stretch = new ContrastStretch();
            stretch.ApplyInPlace(grayscaleImage);


            using (MemoryStream ms = new MemoryStream())
            {
                grayscaleImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                var imageBytes = ms.ToArray();

                // Return the enhanced image as a PNG file
                return new FileContentResult(imageBytes, "image/png");
            }
        }
        private Bitmap ApplyThreshold(Bitmap inputImage, int threshold)
        {
            Bitmap thresholdedImage = new Bitmap(inputImage.Width, inputImage.Height);

            for (int y = 0; y < inputImage.Height; y++)
            {
                for (int x = 0; x < inputImage.Width; x++)
                {
                    Color pixelColor = inputImage.GetPixel(x, y);
                    int averageColor = (pixelColor.R + pixelColor.G + pixelColor.B) / 3;

                    Color thresholdColor = averageColor >= threshold ? Color.White : Color.Black;
                    thresholdedImage.SetPixel(x, y, thresholdColor);
                }
            }

            return thresholdedImage;
        }

        public IActionResult TextLocalization()
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", "khaled-ID.jpg");

            // Load the original image
            Bitmap originalImage = new Bitmap(filePath);

            // Convert the image to grayscale
            Grayscale grayscaleFilter = new Grayscale(0.2125, 0.7154, 0.0721);
            Bitmap grayscaleImage = grayscaleFilter.Apply(originalImage);

            // Apply edge detection (Canny) to the grayscale image
            CannyEdgeDetector edgeDetector = new CannyEdgeDetector();
            Bitmap edgeImage = edgeDetector.Apply(grayscaleImage);

            // Apply blob counter to find contours
            BlobCounter blobCounter = new BlobCounter();
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 10;
            blobCounter.MinWidth = 10;
            blobCounter.ProcessImage(edgeImage);

            // Get the rectangles containing the detected text contours
            Rectangle[] rectangles = blobCounter.GetObjectsRectangles();

            // Draw rectangles on the original image to visualize the detected text
            using (Graphics graphics = Graphics.FromImage(originalImage))
            {
                // Draw rectangles on the original image to visualize the detected text
                foreach (var rect in rectangles)
                {
                    graphics.DrawRectangle(Pens.Red, rect);
                }
            }

            // Convert the processed image to a byte array
            using (MemoryStream ms = new MemoryStream())
            {
                originalImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();

                // Dispose of the bitmap objects to release resources
                originalImage.Dispose();
                grayscaleImage.Dispose();
                edgeImage.Dispose();

                // Return the processed image as a FileContentResult
                return new FileContentResult(imageBytes, "image/png");
            }
        }


        public IActionResult NoiseReducedImage()
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", "khaled-ID.jpg");

            // Load the original image
            Bitmap originalImage = new Bitmap(filePath);

            // Apply noise reduction filters (Gaussian blur, median blur, bilateral filter)
            GaussianBlur gaussianBlur = new GaussianBlur(5, 2); // Adjust parameters as needed
            Bitmap gaussianBlurredImage = gaussianBlur.Apply(originalImage);

            Median medianFilter = new Median();
            Bitmap medianBlurredImage = medianFilter.Apply(originalImage);

            BilateralSmoothing bilateralFilter = new BilateralSmoothing();
            Bitmap bilateralFilteredImage = bilateralFilter.Apply(originalImage);

            // Convert the processed image to a byte array
            using (MemoryStream ms = new MemoryStream())
            {
                gaussianBlurredImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                byte[] imageBytes = ms.ToArray();

                // Dispose of the bitmap objects to release resources
                originalImage.Dispose();
                gaussianBlurredImage.Dispose();
                medianBlurredImage.Dispose();
                bilateralFilteredImage.Dispose();

                // Return the processed image as a FileContentResult
                return new FileContentResult(imageBytes, "image/png");
            }
        }
        [HttpPost]
        public async Task<IActionResult> OcrAsync(ocrModel model) 
        {
            var pic = model.pic;
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "testPics", model.pic.FileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.pic.CopyToAsync(fileStream);
            }

            //OCR//
            using (var engine = new TesseractEngine(@"./tessdata", "ara", EngineMode.Default))
            {
                using (var img = Pix.LoadFromFile(filePath))
                {
                    using (var page = engine.Process(img))
                    {
                        var text = page.GetText();
                        ViewBag.Text = text;
                        ViewBag.Pic = model.pic.FileName;
                    }
                }
            }

            //(1) Image Enhancment
            //   *(Binarization) Convert image into black and White thresholding, adaptive thresholding, or Otsu's method.
            //   *(Noise Reduction) Apply filters like Gaussian blur, median blur, or bilateral filter to reduce noise and improve character detection.
            //   *(Contrast Adjustment): Enhance the contrast of the image to improve the visibility of characters.
            //   *(Skew Correction) : Correct the image's rotation to make text lines horizontal.

            //(2) Image Preprocessing:
            //   *Resizing: Resize the image to an appropriate resolution that maintains text quality and minimizes distortion.
            //   *Denoising: Apply denoising algorithms to reduce unwanted artifacts and improve text clarity.
            //   *Normalization: Normalize the image by adjusting brightness and contrast to make characters stand out.

            //(3) Text Localization: Use Contour detection to locate Text

            //(4) Text Segmentation:

            //Grayscale gray = Grayscale.CommonAlgorithms.BT709;

            //Bitmap originalImage = (Bitmap)Bitmap.FromFile(filePath);
            //Bitmap grayscaleImage = gray.Apply(originalImage);

            //var savedFilePath = "";
            
            //grayscaleImage.Save(savedFilePath, ImageFormat.Jpeg); // Change ImageFormat as needed


            //Threshold thresholdFilter = new Threshold(128);
            //Bitmap binaryImage = thresholdFilter.Apply(grayscaleImage);

            //// Noise Reduction
            //GaussianBlur gaussianFilter = new GaussianBlur(5, 2);
            //Bitmap denoisedImage = gaussianFilter.Apply(originalImage);

            //// Contrast Adjustment
            //ContrastStretch contrastFilter = new ContrastStretch();
            //Bitmap contrastEnhancedImage = contrastFilter.Apply(originalImage);

            //// Resizing
            //int newSizeWidth = 800; // Replace with your desired width
            //int newSizeHeight = 600; // Replace with your desired height
            //ResizeBilinear resizeFilter = new ResizeBilinear(newSizeWidth, newSizeHeight);
            //Bitmap resizedImage = resizeFilter.Apply(originalImage);

            //// Text Localization (Contour Detection)
            //BlobCounter blobCounter = new BlobCounter();
            //blobCounter.ProcessImage(binaryImage);
            //Blob[] blobs = blobCounter.GetObjectsInformation();

            //// Text Segmentation
            //// You can implement text segmentation based on the blob information obtained from contour detection.
            //// ...

            //// Save processed images if needed
            //binaryImage.Save("binarized_image.jpg");
            //denoisedImage.Save("denoised_image.jpg");
            //contrastEnhancedImage.Save("contrast_enhanced_image.jpg");
            //resizedImage.Save("resized_image.jpg");

            //// Dispose of the original image when done
            //originalImage.Dispose();

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}