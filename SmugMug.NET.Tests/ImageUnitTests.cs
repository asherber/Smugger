using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;

namespace SmugMug.NET.Tests
{
    [TestClass]
    public class ImageUnitTests
    {
        private ISmugMugClient api;

        [TestInitialize()]
        public void InitializeAnonymous()
        {
            var mock = new Mock<ISmugMugClient>();

            Image invalidImage = null;
            Image validImage = new Image() { FileName = "ValidFileName.jpg", Title = "Valid Image", Format="JPG" };
            Image unownedImage = new Image() { FileName = "UnownedFileName.png", Title = "Unowned Image", Format="PNG" };
            Image updatedImage = new Image() { FileName = "UnownedFileName.png", Title = "Unowned Image", Format = "PNG", Caption = "Updated caption" };

            ImageUpload invalidImageUpload = null;
            ImageUpload validImageUpload = new ImageUpload() { };

            mock.Setup(api => api.GetImageAsync("ValidImage")).ReturnsAsync(validImage);
            mock.Setup(api => api.GetImageAsync("InvalidImage")).ReturnsAsync(invalidImage);
            mock.Setup(api => api.GetImageAsync("UnownedImage")).ReturnsAsync(unownedImage);

            mock.Setup(api => api.UploadImageAsync("ValidAlbum", "ValidImage")).ReturnsAsync(validImageUpload);
            mock.Setup(api => api.UploadImageAsync("ValidAlbum", "InvalidImage")).ReturnsAsync(invalidImageUpload);

            Node invalidNode = null;
            Node validNode = new Node() { Name = "ValidNode", NodeID = "ABCDE", HasChildren = true };
            mock.Setup(api => api.GetNodeAsync("ValidNode")).ReturnsAsync(validNode);
            mock.Setup(api => api.GetNodeAsync("InvalidNode")).ReturnsAsync(invalidNode);
            mock.Setup(api => api.UploadImageAsync(validNode, "ValidImage")).ReturnsAsync(validImageUpload);
            mock.Setup(api => api.UploadImageAsync(validNode, "InvalidImage")).ReturnsAsync(invalidImageUpload);
            mock.Setup(api => api.UploadImageAsync(invalidNode, "ValidImage")).Throws<ArgumentNullException>();
            mock.Setup(api => api.UploadImageAsync(invalidNode, "InvalidImage")).Throws<ArgumentNullException>();

            Album invalidAlbum = null;
            Album validAlbum = new Album() { Name = "ValidAlbum", ImageCount = 5 };
            mock.Setup(api => api.GetAlbumAsync("ValidAlbum")).ReturnsAsync(validAlbum);
            mock.Setup(api => api.GetAlbumAsync("InvalidAlbum")).ReturnsAsync(invalidAlbum);
            mock.Setup(api => api.UploadImageAsync(validAlbum, "ValidImage")).ReturnsAsync(validImageUpload);
            mock.Setup(api => api.UploadImageAsync(validAlbum, "InvalidImage")).ReturnsAsync(invalidImageUpload);
            mock.Setup(api => api.UploadImageAsync(invalidAlbum, "ValidImage")).Throws<ArgumentNullException>();
            mock.Setup(api => api.UploadImageAsync(invalidAlbum, "InvalidImage")).Throws<ArgumentNullException>();

            mock.Setup(api => api.GetImageAsync(validImageUpload)).ReturnsAsync(validImage);
            mock.Setup(api => api.GetImageAsync(invalidImageUpload)).Throws<ArgumentNullException>();

            mock.Setup(api => api.UpdateImageAsync(validImage, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Caption")))).ReturnsAsync(updatedImage);
            mock.Setup(api => api.UpdateImageAsync(validImage, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateImageAsync((Image)null, It.IsAny<Dictionary<string, string>>())).Throws<ArgumentNullException>();
            mock.Setup(api => api.UpdateImageAsync(validImage, null)).Throws<ArgumentNullException>();

            mock.Setup(api => api.DeleteImageAsync(invalidImage)).Throws<ArgumentNullException>();
            mock.Setup(api => api.DeleteImageAsync(unownedImage)).Throws<HttpRequestException>();

            api = mock.Object;
        }

        [TestMethod]
        public async Task GetImage()
        {
            Image image= await api.GetImageAsync("ValidImage");
            Assert.IsNotNull(image);
            Assert.AreEqual("ValidFileName.jpg", image.FileName);
            Assert.AreEqual("JPG", image.Format);
            Assert.AreEqual("Valid Image", image.Title);
        }

        [TestMethod]
        public async Task GetImage_Invalid()
        {
            Image image = await api.GetImageAsync("InvalidImage");
            Assert.IsNull(image);
        }

        [TestMethod]
        public async Task UploadImage()
        {
            ImageUpload imageUpload = await api.UploadImageAsync("ValidAlbum", "ValidImage");
            Assert.IsNotNull(imageUpload);

            Image image = await api.GetImageAsync(imageUpload);
            Assert.IsNotNull(image);
            Assert.AreEqual("ValidFileName.jpg", image.FileName);
            Assert.AreEqual("JPG", image.Format);
            Assert.AreEqual("Valid Image", image.Title);
        }

        [TestMethod]
        public async Task UploadImage_InvalidImage()
        {
            ImageUpload imageUpload = await api.UploadImageAsync("ValidAlbum", "InvalidImage");
            Assert.IsNull(imageUpload);
        }

        [TestMethod]
        public async Task UploadImage_ByNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Assert.IsNotNull(node);

            ImageUpload imageUpload = await api.UploadImageAsync(node, "ValidImage");
            Assert.IsNotNull(imageUpload);

            Image image = await api.GetImageAsync(imageUpload);
            Assert.IsNotNull(image);
            Assert.AreEqual("ValidFileName.jpg", image.FileName);
            Assert.AreEqual("JPG", image.Format);
            Assert.AreEqual("Valid Image", image.Title);
        }

        [TestMethod]
        public async Task UploadImage_ByNode_InvalidImage()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Assert.IsNotNull(node);

            ImageUpload imageUpload = await api.UploadImageAsync(node, "InvalidImage");
            Assert.IsNull(imageUpload);
        }

        [TestMethod]
        public async Task UploadImage_ByAlbum()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.IsNotNull(album);

            ImageUpload imageUpload = await api.UploadImageAsync(album, "ValidImage");
            Assert.IsNotNull(imageUpload);

            Image image = await api.GetImageAsync(imageUpload);
            Assert.IsNotNull(image);
            Assert.AreEqual("ValidFileName.jpg", image.FileName);
            Assert.AreEqual("JPG", image.Format);
            Assert.AreEqual("Valid Image", image.Title);
        }

        [TestMethod]
        public async Task UploadImage_ByAlbum_InvalidImage()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.IsNotNull(album);

            ImageUpload imageUpload = await api.UploadImageAsync(album, "InvalidImage");
            Assert.IsNull(imageUpload);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UploadImage_ByAlbum_InvalidAlbum()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            Assert.IsNull(album);

            ImageUpload imageUpload = await api.UploadImageAsync(album, "ValidImage");
        }

        [TestMethod]
        public async Task UpdateImage()
        {
            Image image = await api.GetImageAsync("ValidImage");
            Assert.IsNotNull(image);

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Caption", "Updated caption" } };

            Image updatedImage = await api.UpdateImageAsync(image, updates);
            Assert.IsNotNull(updatedImage);
            Assert.AreEqual(updates["Caption"], updatedImage.Caption);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateImage_InvalidAlbum()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Image updatedImage = await api.UpdateImageAsync((Image)null, updates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateImage_InvalidAlbumNullArguments()
        {
            Image image = await api.GetImageAsync("InvalidImage");
            Image updatedImage = await api.UpdateImageAsync(image, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task UpdateImage_InvalidArguments()
        {
            Image image = await api.GetImageAsync("ValidImage");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Image updatedImage = await api.UpdateImageAsync(image, updates);
        }

        [TestMethod]
        public async Task DeleteImage()
        {
            Image image = await api.GetImageAsync("ValidImage");
            await api.DeleteImageAsync(image);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteImage_Invalid()
        {
            Image image = await api.GetImageAsync("InvalidImage");
            await api.DeleteImageAsync(image);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task DeleteImage_Unowned()
        {
            Image image = await api.GetImageAsync("UnownedImage");
            await api.DeleteImageAsync(image);
        }
    }
}
