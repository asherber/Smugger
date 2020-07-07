using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Smugger.Tests
{
    public class AlbumUnitTests
    {
        private ISmugMugClient api;

        public AlbumUnitTests()
        {
            var mock = new Mock<ISmugMugClient>();

            User invalidUser = null;
            User validUser = new User() { Name = "Valid User", NickName = "ValidUser" };
            
            mock.Setup(api => api.GetUserAsync("ValidUser")).ReturnsAsync(validUser);
            mock.Setup(api => api.GetUserAsync("InvalidUser")).ReturnsAsync(invalidUser);

            Folder invalidFolder = null;
            Folder validFolder = new Folder() { Name = "ValidFolder", NodeID = "ABCDE" };

            mock.Setup(api => api.GetFolderAsync("ValidUser", "ValidFolder")).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync("ValidUser", "InvalidFolder")).ReturnsAsync(invalidFolder);

            Album invalidAlbum = null;
            Album validAlbum = new Album() { Name = "ValidAlbum", ImageCount = 5 };
            Album validAlbumWithArguments = new Album() { Name = "ValidAlbum", ImageCount = 5, Description = "Description" };
            Album unownedAlbum = new Album() { Name = "UnownedAlbum", ImageCount = 5, Privacy = PrivacyType.Private };
            Album updatedAlbum = new Album() { Name = "Updated album", ImageCount = 5 };

            mock.Setup(api => api.GetAlbumAsync("ValidAlbum")).ReturnsAsync(validAlbum);
            mock.Setup(api => api.GetAlbumAsync("InvalidAlbum")).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.GetAlbumAsync("UnownedAlbum")).ReturnsAsync(unownedAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "ValidUser", "ValidPath", null)).ReturnsAsync(validAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "ValidUser", "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "ValidUser", "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(validAlbumWithArguments);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "ValidUser", "ValidPath", It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(validAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "ValidUser", "InvalidPath", null)).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "InvalidUser", "ValidPath", null)).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), "InvalidUser", "InvalidPath", null)).ReturnsAsync(invalidAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validUser, "ValidPath", null)).ReturnsAsync(validAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validUser, "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validUser, "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(validAlbumWithArguments);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validUser, "ValidPath", It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(validAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validUser, "InvalidPath", null)).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), invalidUser, "ValidPath", null)).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), invalidUser, "InvalidPath", null)).ReturnsAsync(invalidAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validFolder, null)).ReturnsAsync(validAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validFolder, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validFolder, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(validAlbumWithArguments);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), validFolder, It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(validAlbum);

            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), invalidFolder, null)).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), invalidFolder, null)).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.CreateAlbumAsync(It.IsAny<string>(), invalidFolder, null)).ReturnsAsync(invalidAlbum);

            mock.Setup(api => api.DeleteAlbumAsync(invalidAlbum)).Throws<ArgumentNullException>();
            mock.Setup(api => api.DeleteAlbumAsync(unownedAlbum)).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateAlbumAsync(validAlbum, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Name")))).ReturnsAsync(updatedAlbum);
            mock.Setup(api => api.UpdateAlbumAsync(validAlbum, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateAlbumAsync((Album)null, It.IsAny<Dictionary<string, string>>())).Throws<ArgumentNullException>();
            mock.Setup(api => api.UpdateAlbumAsync(validAlbum, null)).Throws<ArgumentNullException>();

            api = mock.Object;
        }

        [Fact]
        public async Task GetAlbum()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Equal(5, album.ImageCount);
        }
        
        [Fact]
        public async Task GetAlbum_Invalid()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            Assert.Null(album);
        }

        [Fact]
        public async Task GetAlbum_Unowned()
        {
            Album album = await api.GetAlbumAsync("UnownedAlbum");
            Assert.NotNull(album);
            Assert.Equal("UnownedAlbum", album.Name);
            Assert.Equal(5, album.ImageCount);
        }

        [Fact]
        public async Task CreateAlbum_NoArguments()
        {
            Album album = await api.CreateAlbumAsync("NewAlbum", "ValidUser", "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Null(album.Description);
            //TODO: Validate folder Assert.Equal(newAlbum.Uris.Folder)
        }

        [Fact]
        public async Task CreateAlbum_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Album album = await api.CreateAlbumAsync("NewAlbum", "ValidUser", "ValidPath", arguments);
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Equal("Description", album.Description);
            //TODO: Validate folder Assert.Equal(newAlbum.Uris.Folder)
        }

        [Fact]
        public async Task CreateAlbum_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Album album = await api.CreateAlbumAsync("NewAlbum", "ValidUser", "ValidPath", arguments);
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_ByUser_NoArguments()
        {
            User user = await api.GetUserAsync("ValidUser");
            Album album = await api.CreateAlbumAsync("NewAlbum", user, "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Null(album.Description);
            //TODO: Validate folder Assert.Equal(newAlbum.Uris.Folder)
        }

        [Fact]
        public async Task CreateAlbum_ByUser_WithArguments()
        {
            User user = await api.GetUserAsync("ValidUser");
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Album album = await api.CreateAlbumAsync("NewAlbum", user, "ValidPath", arguments);
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Equal("Description", album.Description);
            //TODO: Validate folder Assert.Equal(newAlbum.Uris.Folder)
        }

        [Fact]
        public async Task CreateAlbum_ByUser_WithInvalidArguments()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            User user = await api.GetUserAsync("ValidUser");
            Album album = await api.CreateAlbumAsync("NewAlbum", user, "ValidPath", parameters);
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_ByFolder_NoArguments()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Album album = await api.CreateAlbumAsync("NewAlbum", folder, new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Null(album.Description);
            //TODO: Validate folder Assert.Equal(newAlbum.Uris.Folder)
        }

        [Fact]
        public async Task CreateAlbum_ByFolder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Album album = await api.CreateAlbumAsync("NewAlbum", folder, arguments);
            Assert.NotNull(album);
            Assert.Equal("ValidAlbum", album.Name);
            Assert.Equal("Description", album.Description);
            //TODO: Validate folder Assert.Equal(newAlbum.Uris.Folder)
        }

        [Fact]
        public async Task CreateAlbum_ByFolder_WithInvalidArguments()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Album album = await api.CreateAlbumAsync("NewAlbum", folder, parameters);
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_InvalidAlbum()
        {
            Album album = await api.CreateAlbumAsync("NewAlbum", "ValidUser", "InvalidPath");
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_ByUser_InvalidAlbum()
        {
            User user = await api.GetUserAsync("ValidUser");
            Album album = await api.CreateAlbumAsync("NewAlbum", user, "InvalidPath", null);
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_InvalidUser()
        {
            Album album = await api.CreateAlbumAsync("NewAlbum", "InvalidUser", "InvalidPath", null);
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_ByUser_InvalidUser()
        {
            User invalidUser = await api.GetUserAsync("InvalidUser");
            Album album = await api.CreateAlbumAsync("NewAlbum", invalidUser, "InvalidPath", null);
            Assert.Null(album);
        }

        [Fact]
        public async Task CreateAlbum_ByFolder_InvalidFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            Album album = await api.CreateAlbumAsync("NewAlbum", folder, null);
            Assert.Null(album);
        }

        [Fact]
        public async Task DeleteAlbum()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            await api.DeleteAlbumAsync(album);
        }

        [Fact]
        public async Task DeleteAlbum_Invalid()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.DeleteAlbumAsync(album));

        }

        [Fact]
        public async Task DeleteAlbum_Unowned()
        {
            Album album = await api.GetAlbumAsync("UnownedAlbum");
            await Assert.ThrowsAsync<HttpRequestException>(() => api.DeleteAlbumAsync(album));
        }

        [Fact]
        public async Task UpdateAlbum()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Name", "Updated album" } };

            Album updatedAlbum = await api.UpdateAlbumAsync(album, updates);
            Assert.NotNull(updatedAlbum);
            Assert.Equal("Updated album", updatedAlbum.Name);
        }

        [Fact]
        public async Task UpdateAlbum_InvalidAlbum()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<ArgumentNullException>(() =>api.UpdateAlbumAsync((Album)null, updates));
        }

        [Fact]
        public async Task UpdateAlbum_InvalidAlbumNullArguments()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UpdateAlbumAsync(album, null));
        }

        [Fact]
        public async Task UpdateAlbum_InvalidArguments()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<HttpRequestException>(() => api.UpdateAlbumAsync(album, updates));
        }
    }
}
