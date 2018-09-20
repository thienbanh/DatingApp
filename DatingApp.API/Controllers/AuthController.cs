using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            // tạo configuration để get token
            _config = config;

            _repo = repo;

        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();
            // neu username tồn tại thì trả về bad request
            if (await _repo.UserExists(userForRegisterDto.Username))
                return BadRequest("Username already exists");
            // còn không thì tạo mới 1 username
            var userToCreate = new User
            {
                Username = userForRegisterDto.Username
            };
            // tạo user gồm user và password
            var createUser = await _repo.Register(userToCreate, userForRegisterDto.Password);

            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {
            var userFromRepo = await _repo.Login(userForLoginDto.Username.ToLower(), userForLoginDto.Password);
            // nếu userformrepo khong co thi trả unauthor
            if (userFromRepo == null)
                return Unauthorized();
            //  tạo 2 claim gồn 1 id và 1 username để xác nhận
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name, userFromRepo.Username)
            };
            //  tạo key để sử dụng
            var key = new SymmetricSecurityKey(Encoding.UTF8.
            GetBytes(_config.GetSection("AppSettings:Token").Value));
            // tạo security token 
            // tạo creds để mã hóa key bằng thuật toán Hash(băm)
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            // tạo thông báo mô tả cái claim và hết hạn trong 24h
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // addday hết hạn trong 1 ngày để out ra
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            // tạo 1 JwtSecurityTokenHandler để tạo ra tokendescriptỏ
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            // response token
            return Ok(new{
                token = tokenHandler.WriteToken(token)
            });
        }

    }
}