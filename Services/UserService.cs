using EFpractice.DataBase;
using EFpractice.Models;
using EFpractice.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EFpractice.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _dbContext;

        public UserService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<ActiveUserDto> GetTopCommenters_Slow()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // 1) Eagerly load every user + all their comments → post → category → likes
            var users = _dbContext.Users
                .Include(u => u.Comments)
                  .ThenInclude(c => c.Post)
                    .ThenInclude(p => p.Category)
                .Include(u => u.Comments)
                  .ThenInclude(c => c.Post)
                    .ThenInclude(p => p.Likes)
                .ToList();

            var result = new List<ActiveUserDto>();

            foreach (var u in users)
            {
                // 2) Filter to recent ".NET" comments
                var comments = u.Comments
                    .Where(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                    .ToList();

                var commentsCount = comments.Count;
                if (commentsCount == 0)
                {
                    continue;
                }

                // 3) Top 3 posts by like count
                var topPosts = comments
                    .GroupBy(c => c.Post)
                    .Select(g => new PostDto(
                        g.Key.Id,
                        _dbContext.Likes.Count(l => l.PostId == g.Key.Id)))
                    .OrderByDescending(p => p.LikesCount)
                    .Take(3)
                    .ToList();

                // 4) Latest 2 comments _on those top 3 posts_
                var topPostIds = topPosts.Select(p => p.PostId).ToList();
                var recentTexts = _dbContext.Comments
                    .Where(c =>
                        c.UserId == u.Id &&
                        c.CreatedAt >= since &&
                        topPostIds.Contains(c.PostId))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(2)
                    .Select(c => c.Text)
                    .ToList();

                result.Add(new ActiveUserDto(
                    u.Id, u.Username,
                    commentsCount,
                    topPosts,
                    recentTexts));
            }

            // 5) Final top-5 in memory
            return result
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)
                .ToList();
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization1_PreFilter()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Only users with at least one .NET comment in the window
            var users = _dbContext.Users
                .Where(u => u.Comments
                    .Any(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET"))
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Post)
                        .ThenInclude(p => p.Category)
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Post)
                        .ThenInclude(p => p.Likes)
                .ToList();

            var result = new List<ActiveUserDto>();

            foreach (var u in users)
            {
                // 2) Filter to recent ".NET" comments
                var comments = u.Comments
                    .Where(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                    .ToList();

                var commentsCount = comments.Count;
                if (commentsCount == 0)
                {
                    continue;
                }

                // 3) Top 3 posts by like count
                var topPosts = comments
                    .GroupBy(c => c.Post)
                    .Select(g => new PostDto(
                        g.Key.Id,
                        _dbContext.Likes.Count(l => l.PostId == g.Key.Id)))
                    .OrderByDescending(p => p.LikesCount)
                    .Take(3)
                    .ToList();

                // 4) Latest 2 comments _on those top 3 posts_
                var topPostIds = topPosts.Select(p => p.PostId).ToList();
                var recentTexts = _dbContext.Comments
                    .Where(c =>
                        c.UserId == u.Id &&
                        c.CreatedAt >= since &&
                        topPostIds.Contains(c.PostId))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(2)
                    .Select(c => c.Text)
                    .ToList();

                result.Add(new ActiveUserDto(
                    u.Id, u.Username,
                    commentsCount,
                    topPosts,
                    recentTexts));
            }

            // 5) Final top-5 in memory
            return result
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)
                .ToList();
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization2_LimitUsers()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Compute comment count in the database, then take top 5
            var users = _dbContext.Users
                .Where(u => u.Comments
                    .Any(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET"))
                .OrderByDescending(u => u.Comments
                    .Count(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                )
                .Take(5)
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Post)
                        .ThenInclude(p => p.Category)
                .Include(u => u.Comments)
                    .ThenInclude(c => c.Post)
                        .ThenInclude(p => p.Likes)
                .ToList();



            var result = new List<ActiveUserDto>();

            foreach (var u in users)
            {
                // 2) Filter to recent ".NET" comments
                var comments = u.Comments
                    .Where(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                    .ToList();

                var commentsCount = comments.Count;
                if (commentsCount == 0)
                {
                    continue;
                }

                // 3) Top 3 posts by like count
                var topPosts = comments
                    .GroupBy(c => c.Post)
                    .Select(g => new PostDto(
                        g.Key.Id,
                        _dbContext.Likes.Count(l => l.PostId == g.Key.Id)))
                    .OrderByDescending(p => p.LikesCount)
                    .Take(3)
                    .ToList();

                // 4) Latest 2 comments _on those top 3 posts_
                var topPostIds = topPosts.Select(p => p.PostId).ToList();
                var recentTexts = _dbContext.Comments
                    .Where(c =>
                        c.UserId == u.Id &&
                        c.CreatedAt >= since &&
                        topPostIds.Contains(c.PostId))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(2)
                    .Select(c => c.Text)
                    .ToList();

                result.Add(new ActiveUserDto(
                    u.Id, u.Username,
                    commentsCount,
                    topPosts,
                    recentTexts));
            }

            // 5) Final top-5 in memory
            return result
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)
                .ToList();
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization3_FilterComments()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Only include comments that match our filter
            var users = _dbContext.Users
                .Include(u => u.Comments.Where(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET"))
                .ThenInclude(c => c.Post)
                    .ThenInclude(p => p.Likes)
                .Where(u => u.Comments.Any())
                .OrderByDescending(u => u.Comments
                    .Count(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                )
                .Take(5)
                .ToList();

            var result = new List<ActiveUserDto>();
            foreach (var u in users)
            {
                // u.Comments now only contains recent .NET comments
                var comments = u.Comments.ToList();
                var commentsCount = comments.Count;

                var topPosts = comments
                    .GroupBy(c => c.Post)
                    .Select(g => new PostDto(
                        g.Key.Id,
                        g.Key.Likes.Count))
                    .OrderByDescending(p => p.LikesCount)
                    .Take(3)
                    .ToList();

                var topPostIds = topPosts.Select(p => p.PostId).ToList();
                var recentTexts = _dbContext.Comments
                    .Where(c =>
                        c.UserId == u.Id &&
                        c.CreatedAt >= since &&
                        topPostIds.Contains(c.PostId))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(2)
                    .Select(c => c.Text)
                    .ToList();

                result.Add(new ActiveUserDto(
                    u.Id,
                    u.Username,
                    commentsCount,
                    topPosts,
                    recentTexts));
            }

            return result;
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization4_Projection()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Get users with comment counts and pre-filter them
            var topUsers = _dbContext.Users
                .AsNoTracking() // Add AsNoTracking for read-only operations
                .Where(u => u.Comments.Any(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET"))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    CommentsCount = u.Comments.Count(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                })
                .OrderByDescending(u => u.CommentsCount)
                .Take(5)
                .ToList();

            var result = new List<ActiveUserDto>();

            foreach (var user in topUsers)
            {
                // Get the top posts for this user using projection
                var topPosts = _dbContext.Comments
                    .AsNoTracking()
                    .Where(c =>
                        c.UserId == user.Id &&
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                    .GroupBy(c => c.PostId)
                    .Select(g => new
                    {
                        PostId = g.Key,
                        LikesCount = _dbContext.Posts
                            .Where(p => p.Id == g.Key)
                            .Select(p => p.Likes.Count)
                            .FirstOrDefault()
                    })
                    .OrderByDescending(p => p.LikesCount)
                    .Take(3)
                    .Select(p => new PostDto(p.PostId, p.LikesCount))
                    .ToList();

                // Get the post IDs to use in the next query
                var topPostIds = topPosts.Select(p => p.PostId).ToList();

                // Get the latest comments for this user on their top posts
                var recentTexts = _dbContext.Comments
                    .AsNoTracking()
                    .Where(c =>
                        c.UserId == user.Id &&
                        c.CreatedAt >= since &&
                        topPostIds.Contains(c.PostId))
                    .OrderByDescending(c => c.CreatedAt)
                    .Take(2)
                    .Select(c => c.Text)
                    .ToList();

                // Add to result
                result.Add(new ActiveUserDto(
                    user.Id,
                    user.Username,
                    user.CommentsCount,
                    topPosts,
                    recentTexts
                ));
            }

            return result;
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization5_OneQuery()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            var projected = _dbContext.Users
                // 1) Only users with at least one ".NET" comment in the window
                .Where(u => u.Comments
                    .Any(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET"))

                // 2) Project everything in one go
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    CommentsCount = u.Comments
                        .Count(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET"),

                    // 3) Top 3 posts by like count
                    TopPosts = u.Comments
                        .Where(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET")
                        .GroupBy(c => new { c.Post.Id, c.Post.Likes.Count })
                        .Select(g => new { g.Key.Id, LikesCount = g.Key.Count })
                        .OrderByDescending(p => p.LikesCount)
                        .Take(3),

                    // 4) Latest 2 comments on those top-3 posts
                    RecentComments = u.Comments
                        .Where(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET" &&
                            // subquery filter: only these top IDs
                            u.Comments
                                .Where(d =>
                                    d.CreatedAt >= since &&
                                    d.Post.Category.Name == ".NET")
                                .GroupBy(d => d.PostId)
                                .OrderByDescending(g => g.Count())
                                .Take(3)
                                .Select(g => g.Key)
                                .Contains(c.PostId))
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(2)
                        .Select(c => c.Text)
                })

                // 5) Order & take top 5 users
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)

                // 6) Shape into our DTO
                .Select(x => new ActiveUserDto(
                    x.Id,
                    x.Username,
                    x.CommentsCount,
                    x.TopPosts
                        .Select(p => new PostDto(p.Id, p.LikesCount))
                        .ToList(),
                    x.RecentComments.ToList()
                ))
                .ToList();

            return projected;
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization6_SplitQuery()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            var projected = _dbContext.Users
                // 1) Only users with at least one ".NET" comment in the window
                .Where(u => u.Comments
                    .Any(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET"))

                // 2) Project everything in one go
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    CommentsCount = u.Comments
                        .Count(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET"),

                    // 3) Top 3 posts by like count
                    TopPosts = u.Comments
                        .Where(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET")
                        .GroupBy(c => new { c.Post.Id, c.Post.Likes.Count })
                        .Select(g => new { g.Key.Id, LikesCount = g.Key.Count })
                        .OrderByDescending(p => p.LikesCount)
                        .Take(3),

                    // 4) Latest 2 comments on those top-3 posts
                    RecentComments = u.Comments
                        .Where(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET" &&
                            // subquery filter: only these top IDs
                            u.Comments
                                .Where(d =>
                                    d.CreatedAt >= since &&
                                    d.Post.Category.Name == ".NET")
                                .GroupBy(d => d.PostId)
                                .OrderByDescending(g => g.Count())
                                .Take(3)
                                .Select(g => g.Key)
                                .Contains(c.PostId))
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(2)
                        .Select(c => c.Text)
                })

                // 5) Order & take top 5 users
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)

                // 6) Shape into our DTO
                .Select(x => new ActiveUserDto(
                    x.Id,
                    x.Username,
                    x.CommentsCount,
                    x.TopPosts
                        .Select(p => new PostDto(p.Id, p.LikesCount))
                        .ToList(),
                    x.RecentComments.ToList()
                ))
                .AsSplitQuery()
                .ToList();

            return projected;
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization7_ThreePhaseOptimized()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Phase 1: Get the top 5 users with their comment counts and top posts
            // We first identify the users who commented on .NET posts in the last 7 days
            // and get the top 5 based on comment count
            var topUsers = _dbContext.Users
                .Where(u => u.Comments.Any(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET"))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    CommentsCount = u.Comments.Count(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET")
                })
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)
                .ToList();

            // Get the user IDs to use in subsequent queries
            var userIds = topUsers.Select(u => u.Id).ToArray();

            // Get the top posts for these users by like count
            // This avoids the cartesian explosion from the one-query approach
            var topPostsPerUser = _dbContext.Comments
                .Where(c =>
                    userIds.Contains(c.UserId) &&
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET")
                .GroupBy(c => new { c.UserId, c.PostId })
                .Select(g => new { g.Key.UserId, g.Key.PostId, LikesCount = g.First().Post.Likes.Count })
                .ToList()
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(x => x.LikesCount)
                        .Take(3)
                        .Select(x => new PostDto(x.PostId, x.LikesCount))
                        .ToList()
                );

            // Get the post IDs for the top posts
            var allTopPostIds = topPostsPerUser
                .SelectMany(kvp => kvp.Value.Select(p => p.PostId))
                .Distinct()
                .ToArray();

            // Get the most recent comments for each user on their top posts
            var recentCommentsPerUser = _dbContext.Comments
                .Where(c =>
                    userIds.Contains(c.UserId) &&
                    c.CreatedAt >= since &&
                    allTopPostIds.Contains(c.PostId))
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new { c.UserId, c.Text, c.CreatedAt })
                .ToList()
                .GroupBy(c => c.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(c => c.CreatedAt)
                        .Take(2)
                        .Select(c => c.Text)
                        .ToList()
                );

            // Combine all the data into the final result
            var result = topUsers
                .Select(u => new ActiveUserDto(
                    u.Id,
                    u.Username,
                    u.CommentsCount,
                    topPostsPerUser.TryGetValue(u.Id, out var posts) ? posts : [],
                    recentCommentsPerUser.TryGetValue(u.Id, out var comments) ? comments : []
                ))
                .ToList();

            return result;
        }
        public List<ActiveUserDto> GetTopCommenters_Optimization8_TwoPhaseOptimized()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Phase 1: Fetch the top 5 users plus their top 3 .NET posts and comment counts
            var summaries = _dbContext.Users
                .Where(u => u.Comments
                    .Any(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET"))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    CommentsCount = u.Comments
                        .Count(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET"),
                    TopPosts = u.Comments
                        .Where(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET")
                        .GroupBy(c => c.PostId)
                        .Select(g => new
                        {
                            PostId = g.Key,
                            LikesCount = _dbContext.Likes
                                .Count(l => l.PostId == g.Key)
                        })
                        .OrderByDescending(p => p.LikesCount)
                        .Take(3)
                        .ToList()
                })
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)
                .ToList();

            var userIds = summaries.Select(x => x.Id).ToArray();

            var postIds = summaries
                .SelectMany(s => s.TopPosts.Select(tp => tp.PostId))
                .Distinct()
                .ToArray();

            // Phase 2: Fetch the latest 2 comments per (user, top-post) pair
            var recentCommentsLookup = _dbContext.Comments
                .Where(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET" &&
                    userIds.Contains(c.UserId) &&
                    postIds.Contains(c.PostId))
                .GroupBy(c => new { c.UserId, c.PostId })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.PostId,
                    LatestTwo = g
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(2)
                        .Select(c => c.Text)
                        .ToList()
                })
                .ToList()
                .ToLookup(x => x.UserId, x => x);

            // Compose final DTOs
            var result = summaries
                .Select(s => new ActiveUserDto(
                    s.Id,
                    s.Username,
                    s.CommentsCount,
                    s.TopPosts
                        .Select(tp => new PostDto(tp.PostId, tp.LikesCount))
                        .ToList(),
                    recentCommentsLookup[s.Id]
                        .SelectMany(x => x.LatestTwo)
                        .OrderByDescending(text => text) // ensure latest-first
                        .Take(2)
                        .ToList()
                ))
                .ToList();

            return result;
        }
        public List<ActiveUserDto> GetTopCommenters_Fast()
        {
            var since = DateTime.UtcNow.AddDays(-7);

            // Phase 1: Fetch the top 5 users plus their top 3 .NET posts and comment counts
            var summaries = _dbContext.Users
                .Where(u => u.Comments
                    .Any(c =>
                        c.CreatedAt >= since &&
                        c.Post.Category.Name == ".NET"))
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    CommentsCount = u.Comments
                        .Count(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET"),
                    TopPosts = u.Comments
                        .Where(c =>
                            c.CreatedAt >= since &&
                            c.Post.Category.Name == ".NET")
                        .GroupBy(c => c.PostId)
                        .Select(g => new
                        {
                            PostId = g.Key,
                            LikesCount = _dbContext.Likes
                                .Count(l => l.PostId == g.Key)
                        })
                        .OrderByDescending(p => p.LikesCount)
                        .Take(3)
                        .ToList()
                })
                .OrderByDescending(x => x.CommentsCount)
                .Take(5)
                .ToList();

            var userIds = summaries.Select(x => x.Id).ToArray();

            var postIds = summaries
                .SelectMany(s => s.TopPosts.Select(tp => tp.PostId))
                .Distinct()
                .ToArray();

            // Phase 2: Fetch the latest 2 comments per (user, top-post) pair
            var recentCommentsLookup = _dbContext.Comments
                .Where(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET" &&
                    userIds.Contains(c.UserId) &&
                    postIds.Contains(c.PostId))
                .GroupBy(c => new { c.UserId, c.PostId })
                .Select(g => new
                {
                    g.Key.UserId,
                    g.Key.PostId,
                    LatestTwo = g
                        .OrderByDescending(c => c.CreatedAt)
                        .Take(2)
                        .Select(c => c.Text)
                        .ToList()
                })
                .ToList()
                .ToLookup(x => x.UserId, x => x);

            // Compose final DTOs
            var result = summaries
                .Select(s => new ActiveUserDto(
                    s.Id,
                    s.Username,
                    s.CommentsCount,
                    s.TopPosts
                        .Select(tp => new PostDto(tp.PostId, tp.LikesCount))
                        .ToList(),
                    recentCommentsLookup[s.Id]
                        .SelectMany(x => x.LatestTwo)
                        .OrderByDescending(text => text) // ensure latest-first
                        .Take(2)
                        .ToList()
                ))
                .ToList();

            return result;
        }
    }
}
