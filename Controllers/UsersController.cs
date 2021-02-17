using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Wkhtmltopdf.NetCore;

namespace API.Controllers
{
    // [Route("api/[controller]")]
    // [ApiController]
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository _repository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
         private readonly IConfiguration _config;
        private readonly IGeneratePdf _generatePdf;
        public UsersController(IUserRepository repository, IMapper mapper, 
                    IPhotoService photoService,
                    IConfiguration config, IGeneratePdf generatePdf)
        {
            _generatePdf = generatePdf;
            _config = config;
            _photoService = photoService;
            _mapper = mapper;
            _repository = repository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var users = await _repository.GetMembersAsync(userParams);
            Response.AddingPagingHeader(users.CurrentPage, users.TotalPages,
                    users.PageSize,users.TotalCount);
            return Ok(users);
        }

        [HttpGet("{userName}" , Name="GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string userName)
        {
            var user = await _repository.GetMemberAsync(userName);
            return user;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await _repository.GetUserByUserNameAsync(User.GetUserName());
            _mapper.Map(memberUpdateDto, user);
            _repository.Update(user);
            if (await _repository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to update user.");

        }

        [HttpPost("add-photo")]
        //[AllowAnonymous]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _repository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null)
                return Unauthorized("User not authorized");

            var photo = await _photoService.AddPhotoAsync(HttpContext, file);
            if (user.Photos.Count == 0)
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);
            if (await _repository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { userName = user.UserName},
                         _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Problem adding photo.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _repository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null)
                return Unauthorized("User not authorized");
            
            var photo = user.Photos.FirstOrDefault( o => o.Id == photoId);
            if(photo.IsMain) return BadRequest("This is already your main photo.");
            var currentMain = user.Photos.FirstOrDefault(o => o.IsMain);
            if(currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if(await _repository.SaveAllAsync()) return NoContent();

            return BadRequest("Unable to set main photo.");

        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _repository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null)
                return Unauthorized("User not authorized");
            
            var photo = user.Photos.FirstOrDefault( o => o.Id == photoId);
            if(photo == null) return NotFound();
            if(photo.IsMain) return BadRequest("You cannot delete main photo.");
            _photoService.DeletePhotoAsync(photo.PublicId);
            
            user.Photos.Remove(photo);
            if(await _repository.SaveAllAsync()) return Ok();

            return BadRequest("Unable to delete photo");
        }

        [HttpGet("discount-percentage")]
        public async Task<ActionResult<int>> DiscountPercentage()
        {
            var user = await _repository.GetUserByUserNameAsync(User.GetUserName());
            if (user == null)
                return Unauthorized("User not authorized");
            if(!user.IsPrivileged)
                return BadRequest("Not a privileged user.");
            
            var discount = await _repository.GetDiscount();
            if(discount == null)
                return NotFound("privileged discount not available.");

            return discount.DiscountPercent;

        }

        [HttpPost("export")]
        // public FileResult Export()
        public async Task<IActionResult> Export(EstimationDto estimationDto)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                string fileName = HttpContext.User.GetUserName() + Path.GetRandomFileName() + ".png";
                fileName = "estimation.cshtml";
                string directory = _config["StoredFilesPath"];
                string url = string.Format("{0}://{1}/{2}/{3}", HttpContext.Request.Scheme,
                                        HttpContext.Request.Host.Value, directory, fileName);
                var filePath = Path.Combine(new string[]{ Directory.GetCurrentDirectory(),
                                            directory, fileName});
                string value = System.IO.File.ReadAllText(filePath);
                value = value.Replace("[#GoldPrice#]",string.Format("{0}",estimationDto.GoldPrice));
                value = value.Replace("[#Weight#]",string.Format("{0}",estimationDto.Weight));
                value = value.Replace("[#TotalPrice#]",string.Format("{0}",estimationDto.TotalPrice));
                value = value.Replace("[#Discount#]",string.Format("{0}",estimationDto.Discount));                
                return await _generatePdf.GetPdfViewInHtml(value);
            }
        }
    }
}
