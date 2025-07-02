namespace EFpractice.Models
{
    public record ActiveUserDto(
      int UserId,
      string Username,
      int CommentsCount,
      List<PostDto> TopPosts,
      List<string> RecentComments
  );
}
