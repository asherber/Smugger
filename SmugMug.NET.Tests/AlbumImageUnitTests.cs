using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SmugMug.NET.Tests
{
    [TestClass]
    public class AlbumImageUnitTests
    {
        private ISmugMugClient api;

        [TestInitialize()]
        public void InitializeAnonymous()
        {
            var mock = new Mock<ISmugMugClient>();

            Album invalidAlbum = null;
            Album validAlbum = new Album() { Name = "ValidAlbum", ImageCount = 5 };

            mock.Setup(api => api.GetAlbumAsync("ValidAlbum")).ReturnsAsync(validAlbum);
            mock.Setup(api => api.GetAlbumAsync("InvalidAlbum")).ReturnsAsync(invalidAlbum);

            AlbumImage nullImage = null;
            AlbumImage validImage = new AlbumImage() { FileName = "ValidFileName.jpg", Title = "Valid Image", Format="JPG" };

            mock.Setup(api => api.GetAlbumImageAsync(validAlbum, "ValidImage")).ReturnsAsync(validImage);
            mock.Setup(api => api.GetAlbumImageAsync(validAlbum, "InvalidImage")).ReturnsAsync(nullImage);
            mock.Setup(api => api.GetAlbumImageAsync(invalidAlbum, "ValidImage")).ReturnsAsync(nullImage);
            mock.Setup(api => api.GetAlbumImageAsync(invalidAlbum, "InvalidImage")).ReturnsAsync(nullImage);

            List<AlbumImage> validAlbumImages = new List<AlbumImage>() { new AlbumImage() { FileName = "ValidFileName.jpg", Title = "Valid Image", Format = "JPG" }, new AlbumImage() { FileName = "AnotherValidFileName.jpg", Title = "Another Valid Image", Format = "JPG" }, new AlbumImage() { FileName = "ThirdValidFileName.png", Title = "Third Valid Image", Format = "PNG" } };
            List<AlbumImage> invalidAlbumImages = null;

            mock.Setup(api => api.GetAlbumImagesAsync(validAlbum, It.IsInRange<int>(0, int.MaxValue, Range.Inclusive))).ReturnsAsync(validAlbumImages);
            mock.Setup(api => api.GetAlbumImagesAsync(validAlbum, It.IsInRange<int>(int.MinValue, 0, Range.Inclusive))).ReturnsAsync(invalidAlbumImages);
            mock.Setup(api => api.GetAlbumImagesAsync(invalidAlbum, It.IsInRange<int>(0, int.MaxValue, Range.Inclusive))).ReturnsAsync(invalidAlbumImages);
            mock.Setup(api => api.GetAlbumImagesAsync(invalidAlbum, It.IsInRange<int>(int.MinValue, 0, Range.Inclusive))).ReturnsAsync(invalidAlbumImages);
            api = mock.Object;
        }

        [TestMethod]
        public async Task GetAlbumImage()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.IsNotNull(album);

            AlbumImage image = await api.GetAlbumImageAsync(album, "ValidImage");

            Assert.IsNotNull(image);
            Assert.AreEqual("ValidFileName.jpg", image.FileName);
            Assert.AreEqual("JPG", image.Format);
            Assert.AreEqual("Valid Image", image.Title);
        }

        [TestMethod]
        public async Task GetAlbumImage_Invalid()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.IsNotNull(album);

            AlbumImage image = await api.GetAlbumImageAsync(album, "InvalidImage");
            Assert.IsNull(image);
        }

        [TestMethod]
        public async Task GetAlbumImage_InvalidAlbum()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            AlbumImage image = await api.GetAlbumImageAsync(album, "InvalidImage");
            Assert.IsNull(image);
        }

        [TestMethod]
        public async Task GetAlbumImages()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album);
            Assert.IsNotNull(albumImages);
            Assert.IsTrue(albumImages.Count > 0);
        }

        [TestMethod]
        public async Task GetAlbumImages_InvalidAlbum()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album);
            Assert.IsNull(albumImages);
        }

        [TestMethod]
        public async Task GetAlbumImages_withLimit()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album,3);
            Assert.IsNotNull(albumImages);
            Assert.AreEqual(3, albumImages.Count);
        }

        [TestMethod]
        public async Task GetAlbumImages_withInvalidLimit()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album,-1);
            Assert.IsNull(albumImages);
        }
    }
}
