﻿using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Products_Management.API
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController(IEntityService service, Cloudinary? cloudinary) : ControllerBase
    {
        private readonly IEntityService _service = service;
        private readonly Cloudinary? _cloudinary = cloudinary;

        [HttpGet]
        public async Task<ActionResult<List<EntityResponse>>> GetAll()
        {
            try
            {
                var entities = await _service.GetAllAsync();
                return Ok(entities);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, stackTrace = ex.StackTrace });
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<EntityResponse>> GetById(int id)
        {
            var entity = await _service.GetByIdAsync(id);
            if (entity == null) return NotFound();
            return Ok(entity);
        }
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<EntityResponse>> Create([FromForm] EntityRequest request)
        {
            string? imgUrl = null;

            if (_cloudinary != null && request.ImageUrl != null)
            {
                using var stream = request.ImageUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImageUrl.FileName, stream),
                    Folder = "products"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                imgUrl = uploadResult.SecureUrl?.ToString();
            }

            var result = await _service.AddAsync(request, imgUrl);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<ActionResult<EntityResponse>> Update(int id, [FromForm] EntityRequest request)
        {
            string? imgUrl = null;

            if (_cloudinary != null && request.ImageUrl != null)
            {
                using var stream = request.ImageUrl.OpenReadStream();
                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(request.ImageUrl.FileName, stream),
                    Folder = "products"
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);
                imgUrl = uploadResult.SecureUrl?.ToString();
            }

            var result = await _service.UpdateAsync(id, request, imgUrl);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}