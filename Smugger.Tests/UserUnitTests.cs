using System.Linq;
using Xunit;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Net.Http;

namespace Smugger.Tests
{
    public class UserUnitTests
    {
        private ISmugMugClient api;

        public UserUnitTests()
        {
            var mock = new Mock<ISmugMugClient>();

            SmugMugUri defaultNodeUri = new SmugMugUri() { Uri = "/api/v2/node/ABCDE" };

            User nullUser = null;
            User validUser = new User() { Name = "Valid User", NickName = "ValidUser", Uris = new UserUris { Node = defaultNodeUri } };
            User updatedUser = new User() { Name = "Valid User", NickName = "NickName" };

            mock.Setup(api => api.GetUserAsync("ValidUser")).ReturnsAsync(validUser);
            mock.Setup(api => api.GetUserAsync("InvalidUser")).ReturnsAsync(nullUser);

            mock.Setup(api => api.GetDefaultNodeIDAsync(validUser)).Returns("ABCDE");

            List<Album> validAlbums = new List<Album>() { new Album() { Name = "ValidAlbum", ImageCount = 5 }, new Album() { Name = "AnotherValidAlbum", ImageCount = 10 }, new Album() { Name = "ThirdValidAlbum", ImageCount = 15 } };
            List<Album> invalidAlbums = null;

            mock.Setup(api => api.GetAlbumsAsync(validUser, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(validAlbums);
            mock.Setup(api => api.GetAlbumsAsync(validUser, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbums);
            mock.Setup(api => api.GetAlbumsAsync(nullUser, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbums);
            mock.Setup(api => api.GetAlbumsAsync(nullUser, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbums);

            mock.Setup(api => api.GetFeaturedAlbumsAsync(validUser, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(validAlbums);
            mock.Setup(api => api.GetFeaturedAlbumsAsync(validUser, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbums);
            mock.Setup(api => api.GetFeaturedAlbumsAsync(nullUser, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbums);
            mock.Setup(api => api.GetFeaturedAlbumsAsync(nullUser, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbums);
            
            api = mock.Object;
        }

        [Fact]
        public async Task GetUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Assert.NotNull(user);
            Assert.Equal("Valid User", user.Name);
            Assert.Equal("ValidUser", user.NickName);
            Assert.Equal("/api/v2/node/ABCDE", user.Uris.Node.Uri);
        }

        [Fact]
        public async Task GetUser_Invalid()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Assert.Null(user);
        }

        [Fact]
        public async Task GetUserAlbums()
        {
            User user = await api.GetUserAsync("ValidUser");
            List<Album> albums = await api.GetAlbumsAsync(user);
            Assert.NotNull(albums);
            Assert.True(albums.Count > 0);
        }

        [Fact]
        public async Task GetUserAlbums_Invalid()
        {
            User user = await api.GetUserAsync("InvalidUser");
            List<Album> albums = await api.GetAlbumsAsync(user);
            Assert.Null(albums);
        }

        [Fact]
        public async Task GetUserAlbums_withLimit()
        {
            User user = await api.GetUserAsync("ValidUser");
            List<Album> albums = await api.GetAlbumsAsync(user, 3);
            Assert.NotNull(albums);
            Assert.Equal(3, albums.Count);
        }

        [Fact]
        public async Task GetUserAlbums_withInvalidLimit()
        {
            User user = await api.GetUserAsync("ValidUser");
            List<Album> albums = await api.GetAlbumsAsync(user, -1);
            Assert.Null(albums);
        }

        [Fact]
        public async Task GetUserFeaturedAlbums()
        {
            User user = await api.GetUserAsync("ValidUser");
            List<Album> albums = await api.GetFeaturedAlbumsAsync(user);
            Assert.NotNull(albums);
            Assert.True(albums.Count > 0);
        }

        [Fact]
        public async Task GetUserFeaturedAlbums_withLimit()
        {
            User user = await api.GetUserAsync("ValidUser");
            List<Album> albums = await api.GetFeaturedAlbumsAsync(user, 3);
            Assert.NotNull(albums);
            Assert.Equal(3, albums.Count());
        }

        [Fact]
        public async Task GetUserFeaturedAlbums_withInvalidLimit()
        {
            User user = await api.GetUserAsync("ValidUser");
            List<Album> albums = await api.GetFeaturedAlbumsAsync(user, -1);
            Assert.Null(albums);
        }

        [Fact]
        public async Task GetDefaultNodeId()
        {
            User user = await api.GetUserAsync("ValidUser");
            Assert.NotNull(user);

            string defaultNodeId = api.GetDefaultNodeIDAsync(user);
            Assert.Equal("ABCDE", defaultNodeId);
        }
    }
}
