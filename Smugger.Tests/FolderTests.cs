using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Smugger.Tests
{
    public class FolderTests
    {
        private ISmugMugClient api;

        public FolderTests()
        {
            var mock = new Mock<ISmugMugClient>();

            User invalidUser = null;
            User validUser = new User() { Name = "Valid User", NickName = "ValidUser" };

            mock.Setup(api => api.GetUserAsync("ValidUser")).ReturnsAsync(validUser);
            mock.Setup(api => api.GetUserAsync("InvalidUser")).ReturnsAsync(invalidUser);

            Folder invalidFolder = null;
            Folder validFolder = new Folder() { Name = "ValidFolder", NodeID = "ABCDE" };
            Folder validFolderWithArguments = new Folder() { Name = "ValidFolder", NodeID = "ABCDE", Description = "Description" };
            Folder unownedFolder = new Folder() { Name = "UnownedFolder", NodeID = "ABCDE", Privacy = PrivacyType.Private };
            Folder updatedFolder = new Folder() { Name = "Updated folder", NodeID = "ABCDE" };

            mock.Setup(api => api.GetFolderAsync("ValidUser", null)).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync("ValidUser", "")).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync("ValidUser", "ValidFolder")).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync("ValidUser", "InvalidFolder")).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.GetFolderAsync("ValidUser", "UnownedFolder")).ReturnsAsync(unownedFolder);
            mock.Setup(api => api.GetFolderAsync("InvalidUser", "ValidFolder")).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.GetFolderAsync("InvalidUser", "InvalidFolder")).ReturnsAsync(invalidFolder);

            mock.Setup(api => api.GetFolderAsync(validUser, null)).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync(validUser, "")).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync(validUser, "ValidFolder")).ReturnsAsync(validFolder);
            mock.Setup(api => api.GetFolderAsync(validUser, "InvalidFolder")).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.GetFolderAsync(invalidUser, "ValidFolder")).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.GetFolderAsync(invalidUser, "InvalidFolder")).ReturnsAsync(invalidFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), "ValidPath", null)).ReturnsAsync(validFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(validFolderWithArguments);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), "ValidPath", It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(validFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), "InvalidPath", null)).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), It.IsAny<string>(), "InvalidPath", It.IsNotNull<Dictionary<string, string>>())).ReturnsAsync(invalidFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validUser, "ValidPath", null)).ReturnsAsync(validFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validUser, "InvalidPath", null)).ReturnsAsync(invalidFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validUser, "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validUser, "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(validFolderWithArguments);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validUser, "ValidPath", It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(validFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), invalidUser, "ValidPath", null)).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), invalidUser, "InvalidPath", null)).ReturnsAsync(invalidFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validFolder, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validFolder, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(validFolderWithArguments);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), validFolder, It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(validFolder);

            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), invalidFolder, null)).ReturnsAsync(invalidFolder);
            mock.Setup(api => api.CreateFolderAsync(It.IsAny<string>(), invalidFolder, null)).ReturnsAsync(invalidFolder);

            mock.Setup(api => api.DeleteFolderAsync(invalidFolder)).Throws<ArgumentNullException>();
            mock.Setup(api => api.DeleteFolderAsync(unownedFolder)).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateFolderAsync(validFolder, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Name")))).ReturnsAsync(updatedFolder);
            mock.Setup(api => api.UpdateFolderAsync(validFolder, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateFolderAsync((Folder)null, It.IsAny<Dictionary<string, string>>())).Throws<ArgumentNullException>();
            mock.Setup(api => api.UpdateFolderAsync(validFolder, null)).Throws<ArgumentNullException>();

            api = mock.Object;
        }

        [Fact]
        public async Task GetFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
        }

        [Fact]
        public async Task GetNullFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", null);
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
        }

        [Fact]
        public async Task GetRootFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "");
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
        }

        [Fact]
        public async Task GetFolder_InvalidFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            Assert.Null(folder);
        }

        [Fact]
        public async Task GetFolder_InvalidUser()
        {
            Folder folder = await api.GetFolderAsync("InvalidUser", "ValidFolder");
            Assert.Null(folder);
        }

        [Fact]
        public async Task GetFolder_InvalidFolderAndUser()
        {
            Folder folder = await api.GetFolderAsync("InvalidUser", "InvalidFolder");
            Assert.Null(folder);
        }

        [Fact]
        public async Task GetFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, "ValidFolder");
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
        }

        [Fact]
        public async Task GetNullFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, null);
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
        }

        [Fact]
        public async Task GetRootFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, "");
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
        }

        [Fact]
        public async Task GetFolder_InvalidFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, "InvalidFolder");
            Assert.Null(folder);
        }

        [Fact]
        public async Task GetFolder_InvalidUserByUser()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Folder folder = await api.GetFolderAsync(user, "ValidFolder");
            Assert.Null(folder);
        }

        [Fact]
        public async Task GetFolder_InvalidFolderAndUserByUser()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Folder folder = await api.GetFolderAsync(user, "InvalidFolder");
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_NoArguments()
        {
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
            Assert.Null(folder.Description);
        }

        [Fact]
        public async Task CreateFolder_ByUser_NoArguments()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
            Assert.Null(folder.Description);
        }

        [Fact]
        public async Task CreateFolder_ByFolder_NoArguments()
        {
            Folder validFolder = await api.GetFolderAsync("ValidUser", "");
            Folder folder = await api.CreateFolderAsync("NewFolder", validFolder, new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
            Assert.Null(folder.Description);
        }

        [Fact]
        public async Task CreateFolder_InvalidPath()
        {
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "InvalidPath", null);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_ByUser_InvalidPath()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "InvalidPath", null);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_ByFolder_InvalidFolder()
        {
            Folder invalidFolder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            Folder folder = await api.CreateFolderAsync("NewFolder", invalidFolder, null);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_InvalidUser()
        {
            Folder folder = await api.CreateFolderAsync("NewFolder", "InValidUser", "InvalidPath", null);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_ByUser_InvalidUser()
        {
            User user = await api.GetUserAsync("InValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "InvalidPath", null);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "ValidPath", arguments);
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
            Assert.Equal("Description", folder.Description);
        }

        [Fact]
        public async Task CreateFolder_ByUser_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "ValidPath", arguments);
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
            Assert.Equal("Description", folder.Description);
        }

        [Fact]
        public async Task CreateFolder_ByFolder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Folder validFolder = await api.GetFolderAsync("ValidUser", "");
            Folder folder = await api.CreateFolderAsync("NewFolder", validFolder, arguments);
            Assert.NotNull(folder);
            Assert.Equal("ValidFolder", folder.Name);
            Assert.Equal("ABCDE", folder.NodeID);
            Assert.Equal("Description", folder.Description);
        }

        [Fact]
        public async Task CreateFolder_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "ValidPath", arguments);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_ByUser_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "ValidPath", arguments);
            Assert.Null(folder);
        }

        [Fact]
        public async Task CreateFolder_ByFolder_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder validFolder = await api.GetFolderAsync("ValidUser", "");
            Folder folder = await api.CreateFolderAsync("NewFolder", validFolder, arguments);
            Assert.Null(folder);
        }

        [Fact]
        public async Task DeleteFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            await api.DeleteFolderAsync(folder);
        }

        [Fact]
        public async Task DeleteFolder_Invalid()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            await Assert.ThrowsAsync<ArgumentNullException>(() =>api.DeleteFolderAsync(folder));
        }

        [Fact]
        public async Task DeleteFolder_Unowned()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "UnownedFolder");
            await Assert.ThrowsAsync<HttpRequestException>(() => api.DeleteFolderAsync(folder));
        }

        [Fact]
        public async Task UpdateFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Name", "Updated folder" } };

            Folder updatedFolder = await api.UpdateFolderAsync(folder, updates);
            Assert.NotNull(updatedFolder);
            Assert.Equal("Updated folder", updatedFolder.Name);
        }

        [Fact]
        public async Task UpdateFolder_InvalidFolder()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UpdateFolderAsync((Folder)null, updates));
        }

        [Fact]
        public async Task UpdateFolder_InvalidFolderNullArguments()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            await Assert.ThrowsAsync<ArgumentNullException>(() =>api.UpdateFolderAsync(folder, null));
        }

        [Fact]
        public async Task UpdateFolder_InvalidArguments()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<HttpRequestException>(() => api.UpdateFolderAsync(folder, updates));
        }
    }
}
