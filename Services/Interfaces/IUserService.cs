using EFpractice.Models;

namespace EFpractice.Services.Interfaces
{
    public interface IUserService
    {
        List<ActiveUserDto> GetTopCommenters_Slow();
        List<ActiveUserDto> GetTopCommenters_Optimization1_PreFilter();
        List<ActiveUserDto> GetTopCommenters_Optimization2_LimitUsers();
        List<ActiveUserDto> GetTopCommenters_Optimization3_FilterComments();
        List<ActiveUserDto> GetTopCommenters_Optimization4_Projection();
        List<ActiveUserDto> GetTopCommenters_Optimization5_OneQuery();
        List<ActiveUserDto> GetTopCommenters_Optimization6_SplitQuery();
        List<ActiveUserDto> GetTopCommenters_Optimization7_ThreePhaseOptimized();
        List<ActiveUserDto> GetTopCommenters_Optimization8_TwoPhaseOptimized();
        List<ActiveUserDto> GetTopCommenters_Fast();

    }
}
