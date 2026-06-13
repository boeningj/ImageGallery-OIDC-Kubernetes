using AutoMapper;
using ImageGallery.API.Authorization;
using ImageGallery.API.Services;
using ImageGallery.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ImageGallery.API.Controllers
{
    [Route("api/images")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IGalleryRepository _galleryRepository;        
        private readonly IMapper _mapper;
        private readonly IImageStorageService _imageStorageService;

        public ImagesController(IGalleryRepository galleryRepository, IMapper mapper, IImageStorageService imageStorageService)
        {
            _galleryRepository = galleryRepository ?? throw new ArgumentNullException(nameof(galleryRepository));            
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _imageStorageService = imageStorageService ?? throw new ArgumentNullException(nameof(imageStorageService));
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
            var ownerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (ownerId == null)
            {
                throw new Exception("User identifier is missing from token.");
            }

            // get from repo
            var imagesFromRepo = await _galleryRepository.GetImagesAsync(ownerId);

            // map to model
            var imagesToReturn = _mapper.Map<IEnumerable<Image>>(imagesFromRepo);

            // return
            return Ok(imagesToReturn);
        }

        [HttpGet("{id}", Name = "GetImage")]
        //[Authorize("MustOwnImage")]
        [MustOwnImage]
        public async Task<ActionResult<Image>> GetImage(Guid id)
        {          
            var imageFromRepo = await _galleryRepository.GetImageAsync(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            var imageToReturn = _mapper.Map<Image>(imageFromRepo);

            return Ok(imageToReturn);
        }

        [HttpPost()]
        //[Authorize(Roles = "PayingUser")]
        [Authorize(Policy = "UserCanAddImage")]
        [Authorize(Policy = "ClientApplicationCanWrite")]
        public async Task<ActionResult<Image>> CreateImage([FromBody] ImageForCreation imageForCreation)
        {
            // Automapper maps only the Title in our configuration
            var imageEntity = _mapper.Map<Entities.Image>(imageForCreation);

            // Create an image from the passed-in bytes (Base64), and 
            // set the filename on the image

            /*
            // get this environment's web root path (the path
            // from which static content, like an image, is served)
            var webRootPath = _hostingEnvironment.WebRootPath;

            // create the filename
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            
            // the full file path
            //var filePath = Path.Combine($"{webRootPath}/images/{fileName}");

            // IMPORTANT:
            // Use Path.Combine with correct casing ("Images") instead of string interpolation.
            // - Linux (Docker/K8s) is case-sensitive: "images" ≠ "Images"
            // - String interpolation with "/" can break cross-platform path handling
            // - Path.Combine ensures correct separators and avoids subtle bugs when running in containers
            var filePath = Path.Combine(webRootPath, "Images", fileName);

            // write bytes and auto-close stream
            await System.IO.File.WriteAllBytesAsync(filePath, imageForCreation.Bytes);

            // fill out the filename
            imageEntity.FileName = fileName;
            */

            var fileName = await _imageStorageService.SaveImageAsync(imageForCreation.Bytes);

            imageEntity.FileName = fileName;            

            // set the ownerId on the imageEntity
            var ownerId = User.Claims.FirstOrDefault(c => c.Type == "sub")?.Value;
            if (ownerId == null)
            {
                throw new Exception("User Identifier is missing from token");
            }
            imageEntity.OwnerId = ownerId;

            try
            {
                // Add and save.  
                _galleryRepository.AddImage(imageEntity);
                await _galleryRepository.SaveChangesAsync();
            }
            catch
            {
                try
                {
                    // Compensation:
                    // DB save failed after binary save succeeded.
                    // Attempt to remove the uploaded file so we don't leave orphaned files behind.
                    // NOTE:
                    // This nested try/catch intentionally preserves the ORIGINAL
                    // database exception. If compensation cleanup also fails, we do
                    // not want the cleanup exception to mask the primary failure cause.
                    await _imageStorageService.DeleteImageAsync(fileName);
                }
                catch
                {
                    // Optional:  Log compensation failure later
                }
                throw;
            }

            var imageToReturn = _mapper.Map<Image>(imageEntity);

            return CreatedAtRoute("GetImage", new { id = imageToReturn.Id }, imageToReturn);
        }

        [HttpDelete("{id}")]
        [Authorize("MustOwnImage")]
        public async Task<IActionResult> DeleteImage(Guid id)
        {            
            var imageFromRepo = await _galleryRepository.GetImageAsync(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            try
            {
                // Delete the physical binary file first.
                //
                // IMPORTANT:
                // We intentionally remove the binary image file before deleting the
                // database metadata so we don't end up with orphaned files
                // or objects in storage.
                //
                // If binary deletion fails, we abort the operation and keep
                // the DB row intact so the system remains internally consistent.
                await _imageStorageService.DeleteImageAsync(imageFromRepo.FileName);
                _galleryRepository.DeleteImage(imageFromRepo);
                await _galleryRepository.SaveChangesAsync();
            }
            catch
            {
                // Optional future improvement:  Add structured logging here.
                throw;
            }

            return NoContent();
        }

        [HttpPut("{id}")]
        [Authorize("MustOwnImage")]
        public async Task<IActionResult> UpdateImage(Guid id, 
            [FromBody] ImageForUpdate imageForUpdate)
        {
            var imageFromRepo = await _galleryRepository.GetImageAsync(id);
            if (imageFromRepo == null)
            {
                return NotFound();
            }

            _mapper.Map(imageForUpdate, imageFromRepo);

            _galleryRepository.UpdateImage(imageFromRepo);

            await _galleryRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/file")]
        [MustOwnImage]
        public async Task<IActionResult> GetImageFile(Guid id)
        {
            var imageFromRepo = await _galleryRepository.GetImageAsync(id);

            if (imageFromRepo == null)
            {
                return NotFound();
            }

            var imageStream = await _imageStorageService.GetImageAsync(imageFromRepo.FileName);

            return File(imageStream, "image/jpeg");
        }
    }
}