using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using mediaAPI;
using mediaAPI.Models;
using mediaAPI.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace mediaAPI.Tests
{
    public class MediaApiFactory : WebApplicationFactory<TestAppConfiguration>
    {
        public void ConfigureServices(IServiceCollection services)
        {
            var postRepository = new Mock<IPostRepository>();
            postRepository.Setup(x => x.GetAllPostsAsync()).ReturnsAsync(new List<Post>
    {
        new Post { Id = 1, Title = "Test title 1", Body = "Test body 1", Author = 1 },
        new Post { Id = 2, Title = "Test title 2", Body = "Test body 2", Author = 2 }
    });
            postRepository.Setup(x => x.CreatePostAsync(It.IsAny<Post>())).ReturnsAsync(1);
            postRepository.Setup(x => x.GetPostByIdAsync(1)).ReturnsAsync(new Post { Title = "Test title", Body = "Test body", Author = 1 });
            services.AddSingleton(postRepository.Object);

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Username = "TestUser" });
            userRepository.Setup(x => x.GetUserByIdAsync(2)).ReturnsAsync(new User { Id = 2, Username = "TestUser2" });

            services.AddSingleton(userRepository.Object);
        }
        public void ConfigureServicesScenarioForLikes(IServiceCollection services)
        {
            var postRepository = new Mock<IPostRepository>();
            postRepository.Setup(x => x.CreatePostAsync(It.IsAny<Post>())).ReturnsAsync(1);
            postRepository.Setup(x => x.GetPostByIdAsync(1)).ReturnsAsync(new Post { Title = "Test title", Body = "Test body", Author = 1 });
            services.AddSingleton(postRepository.Object);

            var userRepository = new Mock<IUserRepository>();
            userRepository.Setup(x => x.GetUserByIdAsync(1)).ReturnsAsync(new User { Id = 1, Username = "TestUser" });
            services.AddSingleton(userRepository.Object);
        }
        public void ConfigureServicesScenarioEmptyPostRepo(IServiceCollection services)
        {
            var postRepository = new Mock<IPostRepository>();
            postRepository.Setup(x => x.GetAllPostsAsync()).ReturnsAsync(new List<Post>{});
            services.AddSingleton(postRepository.Object);

        }

    }
}



