using System;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Net.Http;
using Xunit;

namespace Smugger.Tests
{
    public class ImageTests
    {
        private ISmugMugClient api;

        public ImageTests()
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

        [Fact]
        public async Task GetImage()
        {
            Image image= await api.GetImageAsync("ValidImage");
            Assert.NotNull(image);
            Assert.Equal("ValidFileName.jpg", image.FileName);
            Assert.Equal("JPG", image.Format);
            Assert.Equal("Valid Image", image.Title);
        }

        [Fact]
        public async Task GetImage_Invalid()
        {
            Image image = await api.GetImageAsync("InvalidImage");
            Assert.Null(image);
        }

        [Fact]
        public async Task UploadImage()
        {
            ImageUpload imageUpload = await api.UploadImageAsync("ValidAlbum", "ValidImage");
            Assert.NotNull(imageUpload);

            Image image = await api.GetImageAsync(imageUpload);
            Assert.NotNull(image);
            Assert.Equal("ValidFileName.jpg", image.FileName);
            Assert.Equal("JPG", image.Format);
            Assert.Equal("Valid Image", image.Title);
        }

        [Fact]
        public async Task UploadImage_InvalidImage()
        {
            ImageUpload imageUpload = await api.UploadImageAsync("ValidAlbum", "InvalidImage");
            Assert.Null(imageUpload);
        }

        [Fact]
        public async Task UploadImage_ByNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Assert.NotNull(node);

            ImageUpload imageUpload = await api.UploadImageAsync(node, "ValidImage");
            Assert.NotNull(imageUpload);

            Image image = await api.GetImageAsync(imageUpload);
            Assert.NotNull(image);
            Assert.Equal("ValidFileName.jpg", image.FileName);
            Assert.Equal("JPG", image.Format);
            Assert.Equal("Valid Image", image.Title);
        }

        [Fact]
        public async Task UploadImage_ByNode_InvalidImage()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Assert.NotNull(node);

            ImageUpload imageUpload = await api.UploadImageAsync(node, "InvalidImage");
            Assert.Null(imageUpload);
        }

        [Fact]
        public async Task UploadImage_ByAlbum()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.NotNull(album);

            ImageUpload imageUpload = await api.UploadImageAsync(album, "ValidImage");
            Assert.NotNull(imageUpload);

            Image image = await api.GetImageAsync(imageUpload);
            Assert.NotNull(image);
            Assert.Equal("ValidFileName.jpg", image.FileName);
            Assert.Equal("JPG", image.Format);
            Assert.Equal("Valid Image", image.Title);
        }

        [Fact]
        public async Task UploadImage_ByAlbum_InvalidImage()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.NotNull(album);

            ImageUpload imageUpload = await api.UploadImageAsync(album, "InvalidImage");
            Assert.Null(imageUpload);
        }

        [Fact]
        public async Task UploadImage_ByAlbum_InvalidAlbum()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            Assert.Null(album);

            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UploadImageAsync(album, "ValidImage"));
        }

        [Fact]
        public async Task UpdateImage()
        {
            Image image = await api.GetImageAsync("ValidImage");
            Assert.NotNull(image);

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Caption", "Updated caption" } };

            Image updatedImage = await api.UpdateImageAsync(image, updates);
            Assert.NotNull(updatedImage);
            Assert.Equal(updates["Caption"], updatedImage.Caption);
        }

        [Fact]
        public async Task UpdateImage_InvalidAlbum()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UpdateImageAsync((Image)null, updates));
        }

        [Fact]
        public async Task UpdateImage_InvalidAlbumNullArguments()
        {
            Image image = await api.GetImageAsync("InvalidImage");
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UpdateImageAsync(image, null));
        }

        [Fact]
        public async Task UpdateImage_InvalidArguments()
        {
            Image image = await api.GetImageAsync("ValidImage");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<HttpRequestException>(() => api.UpdateImageAsync(image, updates));
        }

        [Fact]
        public async Task DeleteImage()
        {
            Image image = await api.GetImageAsync("ValidImage");
            await api.DeleteImageAsync(image);
        }

        [Fact]
        public async Task DeleteImage_Invalid()
        {
            Image image = await api.GetImageAsync("InvalidImage");
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.DeleteImageAsync(image));
        }

        [Fact]
        public async Task DeleteImage_Unowned()
        {
            Image image = await api.GetImageAsync("UnownedImage");
            await Assert.ThrowsAsync<HttpRequestException>(() => api.DeleteImageAsync(image));
        }
    }
}
