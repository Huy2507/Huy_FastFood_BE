using Huy_FastFood_BE.DTOs;
using Huy_FastFood_BE.Models;
using Huy_FastFood_BE.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Huy_FastFood_BE.Controllers.Admin
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> CreateBanner([FromBody] BannerDTO dto)
        {
            try
            {
                var banner = new Banner
                {
                    BannerImg = dto.BannerImg,
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
        public async Task<IActionResult> UpdateBanner(int id, [FromBody] BannerDTO dto)
        {
            try
            {
                var existingBanner = await _dbContext.Banners.FindAsync(id);
                if (existingBanner == null)
                    return NotFound(new { message = "Banner not found." });

                existingBanner.BannerImg = dto.BannerImg;
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
