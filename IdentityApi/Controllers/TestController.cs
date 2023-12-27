using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdentityApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TestController : ControllerBase
    {

        [HttpGet("Admin")]
        [Authorize(Roles = "Admin")]
        public string GetAdmin()
        {
            return "You Hit Me Admin!";
        }
        [HttpGet("User")]
        [Authorize(Roles = "User")]
        public string GetUser()
        {
            return "You Hit Me User!";
        }
    }
}
