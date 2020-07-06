using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmugMug.NET.Tests
{
    [TestClass]
    public class FolderUnitTests
    {
        private ISmugMugClient api;

        [TestInitialize()]
        public void InitializeAnonymous()
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

        [TestMethod]
        public async Task GetFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
        }

        [TestMethod]
        public async Task GetNullFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", null);
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
        }

        [TestMethod]
        public async Task GetRootFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "");
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
        }

        [TestMethod]
        public async Task GetFolder_InvalidFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task GetFolder_InvalidUser()
        {
            Folder folder = await api.GetFolderAsync("InvalidUser", "ValidFolder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task GetFolder_InvalidFolderAndUser()
        {
            Folder folder = await api.GetFolderAsync("InvalidUser", "InvalidFolder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task GetFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, "ValidFolder");
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
        }

        [TestMethod]
        public async Task GetNullFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, null);
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
        }

        [TestMethod]
        public async Task GetRootFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, "");
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
        }

        [TestMethod]
        public async Task GetFolder_InvalidFolderByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.GetFolderAsync(user, "InvalidFolder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task GetFolder_InvalidUserByUser()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Folder folder = await api.GetFolderAsync(user, "ValidFolder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task GetFolder_InvalidFolderAndUserByUser()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Folder folder = await api.GetFolderAsync(user, "InvalidFolder");
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_NoArguments()
        {
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
            Assert.IsNull(folder.Description);
        }

        [TestMethod]
        public async Task CreateFolder_ByUser_NoArguments()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
            Assert.IsNull(folder.Description);
        }

        [TestMethod]
        public async Task CreateFolder_ByFolder_NoArguments()
        {
            Folder validFolder = await api.GetFolderAsync("ValidUser", "");
            Folder folder = await api.CreateFolderAsync("NewFolder", validFolder, new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
            Assert.IsNull(folder.Description);
        }

        [TestMethod]
        public async Task CreateFolder_InvalidPath()
        {
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "InvalidPath", null);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_ByUser_InvalidPath()
        {
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "InvalidPath", null);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_ByFolder_InvalidFolder()
        {
            Folder invalidFolder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            Folder folder = await api.CreateFolderAsync("NewFolder", invalidFolder, null);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_InvalidUser()
        {
            Folder folder = await api.CreateFolderAsync("NewFolder", "InValidUser", "InvalidPath", null);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_ByUser_InvalidUser()
        {
            User user = await api.GetUserAsync("InValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "InvalidPath", null);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "ValidPath", arguments);
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
            Assert.AreEqual("Description", folder.Description);
        }

        [TestMethod]
        public async Task CreateFolder_ByUser_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "ValidPath", arguments);
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
            Assert.AreEqual("Description", folder.Description);
        }

        [TestMethod]
        public async Task CreateFolder_ByFolder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Folder validFolder = await api.GetFolderAsync("ValidUser", "");
            Folder folder = await api.CreateFolderAsync("NewFolder", validFolder, arguments);
            Assert.IsNotNull(folder);
            Assert.AreEqual("ValidFolder", folder.Name);
            Assert.AreEqual("ABCDE", folder.NodeID);
            Assert.AreEqual("Description", folder.Description);
        }

        [TestMethod]
        public async Task CreateFolder_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder folder = await api.CreateFolderAsync("NewFolder", "ValidUser", "ValidPath", arguments);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_ByUser_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            User user = await api.GetUserAsync("ValidUser");
            Folder folder = await api.CreateFolderAsync("NewFolder", user, "ValidPath", arguments);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task CreateFolder_ByFolder_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder validFolder = await api.GetFolderAsync("ValidUser", "");
            Folder folder = await api.CreateFolderAsync("NewFolder", validFolder, arguments);
            Assert.IsNull(folder);
        }

        [TestMethod]
        public async Task DeleteFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            await api.DeleteFolderAsync(folder);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteFolder_Invalid()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            await api.DeleteFolderAsync(folder);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task DeleteFolder_Unowned()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "UnownedFolder");
            await api.DeleteFolderAsync(folder);
        }

        [TestMethod]
        public async Task UpdateFolder()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Name", "Updated folder" } };

            Folder updatedFolder = await api.UpdateFolderAsync(folder, updates);
            Assert.IsNotNull(updatedFolder);
            Assert.AreEqual("Updated folder", updatedFolder.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateFolder_InvalidFolder()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder updatedFolder = await api.UpdateFolderAsync((Folder)null, updates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateFolder_InvalidFolderNullArguments()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "InvalidFolder");
            Folder updatedFolder = await api.UpdateFolderAsync(folder, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task UpdateFolder_InvalidArguments()
        {
            Folder folder = await api.GetFolderAsync("ValidUser", "ValidFolder");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Folder updatedFolder = await api.UpdateFolderAsync(folder, updates);
        }
    }
}
