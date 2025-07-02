using EFpractice.Models;
using EFpractice.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;


namespace EFpractice.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("top-commenters-slow")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Slow()
        {
            var result = _userService.GetTopCommenters_Slow();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization1")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization1()
        {
            var result = _userService.GetTopCommenters_Optimization1_PreFilter();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization2")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization2()
        {
            var result = _userService.GetTopCommenters_Optimization2_LimitUsers();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization3")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization3()
        {
            var result = _userService.GetTopCommenters_Optimization3_FilterComments();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization4")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization4()
        {
            var result = _userService.GetTopCommenters_Optimization4_Projection();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization5")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization5()
        {
            var result = _userService.GetTopCommenters_Optimization5_OneQuery();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization6")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization6()
        {
            var result = _userService.GetTopCommenters_Optimization6_SplitQuery();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization7")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization7()
        {
            var result = _userService.GetTopCommenters_Optimization7_ThreePhaseOptimized();
            return Ok(result);
        }
        [HttpGet("top-commenters-optimization8")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Optimization8()
        {
            var result = _userService.GetTopCommenters_Optimization8_TwoPhaseOptimized();
            return Ok(result);
        }

        [HttpGet("top-commenters-fast")]
        public ActionResult<List<ActiveUserDto>> GetTopCommenters_Fast()
        {
            var result = _userService.GetTopCommenters_Fast();
            return Ok(result);
        }
    }
}
