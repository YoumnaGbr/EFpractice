using EFpractice.DataBase.Entites;
using EFpractice.DataBase;
using Microsoft.EntityFrameworkCore;
using Bogus;

namespace EFpractice.Services
{
    public static class DatabaseSeedService
    {
        private const int RandomSeed = 42;

        public static async Task SeedAsync(ApplicationDbContext dbContext)
        {
            await dbContext.Database.MigrateAsync();

            if (await dbContext.Users.AnyAsync()) return;

            Randomizer.Seed = new Random(RandomSeed);

            await SeedCategoriesAsync(dbContext);
            var users = await SeedUsersAsync(dbContext, 100);
            var categories = await dbContext.Categories.ToListAsync();
            var posts = await SeedPostsAsync(dbContext, users, categories);
            await SeedCommentsAsync(dbContext, posts, users);
            await SeedLikesAsync(dbContext, posts);
        }

        private static async Task SeedCategoriesAsync(ApplicationDbContext dbContext)
        {
            var categoryNames = new List<string>
            {
                ".NET", "JavaScript", "Python", "Java", "C++", "Go",
                "Rust", "Ruby", "PHP", "Swift", "Kotlin"
            };

            var categories = categoryNames.Select(name => new Category { Name = name }).ToList();
            dbContext.Categories.AddRange(categories);
            await dbContext.SaveChangesAsync();
        }

        private static async Task<List<User>> SeedUsersAsync(ApplicationDbContext dbContext, int count)
        {
            var userFaker = new Faker<User>().RuleFor(u => u.Username, f => f.Internet.UserName());
            var users = userFaker.Generate(count);
            dbContext.Users.AddRange(users);
            await dbContext.SaveChangesAsync();
            return users;
        }

        private static async Task<List<Post>> SeedPostsAsync(ApplicationDbContext dbContext, List<User> users, List<Category> categories)
        {
            var faker = new Faker();
            var dotNetCategory = categories.First(c => c.Name == ".NET");
            var posts = new List<Post>();

            for (int i = 0; i < 3000; i++)
            {
                posts.Add(new Post
                {
                    Content = faker.Lorem.Paragraphs(3),
                    UserId = faker.PickRandom(users).Id,
                    CategoryId = dotNetCategory.Id
                });
            }

            var otherCategories = categories.Where(c => c.Id != dotNetCategory.Id).ToList();
            for (int i = 0; i < 2000; i++)
            {
                posts.Add(new Post
                {
                    Content = faker.Lorem.Paragraphs(3),
                    UserId = faker.PickRandom(users).Id,
                    CategoryId = faker.PickRandom(otherCategories).Id
                });
            }

            dbContext.Posts.AddRange(posts);
            await dbContext.SaveChangesAsync();
            return posts;
        }

        private static async Task SeedCommentsAsync(ApplicationDbContext dbContext, List<Post> posts, List<User> users)
        {
            var faker = new Faker();
            const int batchSize = 1000;
            var comments = new List<Comment>();

            foreach (var post in posts)
            {
                for (int i = 0; i < 50; i++)
                {
                    comments.Add(new Comment
                    {
                        PostId = post.Id,
                        UserId = faker.PickRandom(users).Id,
                        Text = faker.Lorem.Paragraph(),
                        CreatedAt = faker.Date.Recent(14)
                    });
                }
            }

            for (int i = 0; i < comments.Count; i += batchSize)
            {
                var batch = comments.Skip(i).Take(batchSize).ToList();
                dbContext.Comments.AddRange(batch);
                await dbContext.SaveChangesAsync();
            }
        }

        private static async Task SeedLikesAsync(ApplicationDbContext dbContext, List<Post> posts)
        {
            var faker = new Faker();
            const int batchSize = 1000;
            var likes = new List<Like>();

            foreach (var post in posts)
            {
                for (int i = 0; i < 100; i++)
                {
                    likes.Add(new Like { PostId = post.Id });
                }
            }

            for (int i = 0; i < likes.Count; i += batchSize)
            {
                var batch = likes.Skip(i).Take(batchSize).ToList();
                dbContext.Likes.AddRange(batch);
                await dbContext.SaveChangesAsync();
            }
        }
    }
}