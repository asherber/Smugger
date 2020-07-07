using System;
using Moq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Xunit;

namespace Smugger.Tests
{
    public class AlbumImageTests
    {
        private ISmugMugClient api;

        public AlbumImageTests()
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

            mock.Setup(api => api.GetAlbumImagesAsync(validAlbum, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(validAlbumImages);
            mock.Setup(api => api.GetAlbumImagesAsync(validAlbum, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbumImages);
            mock.Setup(api => api.GetAlbumImagesAsync(invalidAlbum, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbumImages);
            mock.Setup(api => api.GetAlbumImagesAsync(invalidAlbum, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidAlbumImages);
            api = mock.Object;
        }

        [Fact]
        public async Task GetAlbumImage()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.NotNull(album);

            AlbumImage image = await api.GetAlbumImageAsync(album, "ValidImage");

            Assert.NotNull(image);
            Assert.Equal("ValidFileName.jpg", image.FileName);
            Assert.Equal("JPG", image.Format);
            Assert.Equal("Valid Image", image.Title);
        }

        [Fact]
        public async Task GetAlbumImage_Invalid()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            Assert.NotNull(album);

            AlbumImage image = await api.GetAlbumImageAsync(album, "InvalidImage");
            Assert.Null(image);
        }

        [Fact]
        public async Task GetAlbumImage_InvalidAlbum()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            AlbumImage image = await api.GetAlbumImageAsync(album, "InvalidImage");
            Assert.Null(image);
        }

        [Fact]
        public async Task GetAlbumImages()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album);
            Assert.NotNull(albumImages);
            Assert.True(albumImages.Count > 0);
        }

        [Fact]
        public async Task GetAlbumImages_InvalidAlbum()
        {
            Album album = await api.GetAlbumAsync("InvalidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album);
            Assert.Null(albumImages);
        }

        [Fact]
        public async Task GetAlbumImages_withLimit()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album,3);
            Assert.NotNull(albumImages);
            Assert.Equal(3, albumImages.Count);
        }

        [Fact]
        public async Task GetAlbumImages_withInvalidLimit()
        {
            Album album = await api.GetAlbumAsync("ValidAlbum");
            List<AlbumImage> albumImages = await api.GetAlbumImagesAsync(album,-1);
            Assert.Null(albumImages);
        }
    }
}
