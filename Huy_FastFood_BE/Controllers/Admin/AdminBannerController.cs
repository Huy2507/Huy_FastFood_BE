using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminBannerController : ControllerBase
    {
        private readonly AppDbContext _dbContext;

        public AdminBannerController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // GET: api/Banner
        [HttpGet]
        public async Task<IActionResult> GetAllBanners()
        {
            try
            {
                var banners = await _dbContext.Banners
                    .Select(b => new BannerDTO
                    {
                        Id = b.Id,
                        BannerImg = b.BannerImg,
                        Title = b.Title,
                        Description = b.Description,
                        LinkUrl = b.LinkUrl,
                        SeoTitle = b.SeoTitle,
                        SeoDescript = b.SeoDescript,
                        SeoKeywords = b.SeoKeywords,
                        Slug = b.Slug,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate
                    })
                    .ToListAsync();

                return Ok(banners);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // GET: api/Banner/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetBannerById(int id)
        {
            try
            {
                var banner = await _dbContext.Banners
                    .Where(b => b.Id == id)
                    .Select(b => new BannerDTO
                    {
                        Id = b.Id,
                        BannerImg = b.BannerImg,
                        Title = b.Title,
                        Description = b.Description,
                        LinkUrl = b.LinkUrl,
                        SeoTitle = b.SeoTitle,
                        SeoDescript = b.SeoDescript,
                        SeoKeywords = b.SeoKeywords,
                        Slug = b.Slug,
                        StartDate = b.StartDate,
                        EndDate = b.EndDate
                    })
                    .FirstOrDefaultAsync();

                if (banner == null)
                    return NotFound(new { message = "Banner not found." });

                return Ok(banner);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        // POST: api/Banner
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateBanner([FromForm] BannerCreateDTO dto)
        {
            try
            {
                string bannerImgUrl = null;

                // Xử lý file ảnh tải lên
                if (dto.BannerImgFile != null && dto.BannerImgFile.Length > 0)
                {
                    // Đường dẫn thư mục lưu ảnh
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Banner");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.BannerImgFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    // Lưu file ảnh
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.BannerImgFile.CopyToAsync(stream);
                    }

                    // Lấy đường dẫn URL của ảnh
                    bannerImgUrl = $"/Images/Banner/{fileName}";
                }

                var banner = new Banner
                {
                    BannerImg = bannerImgUrl,
                    Title = dto.Title,
                    Description = dto.Description,
                    LinkUrl = dto.LinkUrl,
                    SeoTitle = dto.SeoTitle,
                    SeoDescript = dto.SeoDescript,
                    SeoKeywords = dto.SeoKeywords,
                    Slug = SlugHelper.GenerateSlug(dto.Title),
                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate
                };

                await _dbContext.Banners.AddAsync(banner);
                await _dbContext.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBannerById), new { id = banner.Id }, new { message = "Banner created successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        // PUT: api/Banner/{id}
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateBanner(int id, [FromForm] BannerCreateDTO dto)
        {
            try
            {
                var existingBanner = await _dbContext.Banners.FindAsync(id);
                if (existingBanner == null)
                    return NotFound(new { message = "Banner not found." });

                string bannerImgUrl = existingBanner.BannerImg; // Giữ ảnh cũ nếu không tải ảnh mới

                // Xử lý file ảnh tải lên
                if (dto.BannerImgFile != null && dto.BannerImgFile.Length > 0)
                {
                    // Đường dẫn thư mục lưu ảnh
                    var imagesFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Images", "Banner");
                    if (!Directory.Exists(imagesFolder))
                    {
                        Directory.CreateDirectory(imagesFolder);
                    }

                    // Tạo tên file duy nhất
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(dto.BannerImgFile.FileName);
                    var filePath = Path.Combine(imagesFolder, fileName);

                    // Lưu file ảnh mới
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await dto.BannerImgFile.CopyToAsync(stream);
                    }

                    // Xóa ảnh cũ nếu tồn tại
                    if (!string.IsNullOrEmpty(existingBanner.BannerImg))
                    {
                        var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingBanner.BannerImg.TrimStart('/'));
                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    // Cập nhật đường dẫn ảnh mới
                    bannerImgUrl = $"/Images/Banner/{fileName}";
                }

                existingBanner.BannerImg = bannerImgUrl;
                existingBanner.Title = dto.Title;
                existingBanner.Description = dto.Description;
                existingBanner.LinkUrl = dto.LinkUrl;
                existingBanner.SeoTitle = dto.SeoTitle;
                existingBanner.SeoDescript = dto.SeoDescript;
                existingBanner.SeoKeywords = dto.SeoKeywords;
                existingBanner.Slug = SlugHelper.GenerateSlug(dto.Title);
                existingBanner.StartDate = dto.StartDate;
                existingBanner.EndDate = dto.EndDate;

                _dbContext.Banners.Update(existingBanner);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Banner updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }


        // DELETE: api/Banner/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            try
            {
                var banner = await _dbContext.Banners.FindAsync(id);
                if (banner == null)
                    return NotFound(new { message = "Banner not found." });

                // Xóa ảnh nếu tồn tại
                if (!string.IsNullOrEmpty(banner.BannerImg))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", banner.BannerImg.TrimStart('/'));
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                // Xóa banner khỏi database
                _dbContext.Banners.Remove(banner);
                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Banner deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

    }
}
