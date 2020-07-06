﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmugMug.NET.Tests
{
    [TestClass]
    public class UserProfileUnitTests
    {
        private ISmugMugClient api;

        [TestInitialize()]
        public void InitializeAnonymous()
        {
            var mock = new Mock<ISmugMugClient>();

            UserProfile nullUserProfile = null;
            UserProfile validUserProfile = new UserProfile() { DisplayName = "Valid User", BioText = "Valid bio" };
            UserProfile updatedUserProfile = new UserProfile() { DisplayName = "Valid User", BioText = "Updated bio" };

            User nullUser = null;
            User validUser = new User() { Name = "Valid User", NickName = "ValidUser" };

            mock.Setup(api => api.GetUserAsync("ValidUser")).ReturnsAsync(validUser);
            mock.Setup(api => api.GetUserAsync("InvalidUser")).ReturnsAsync(nullUser);

            mock.Setup(api => api.GetUserProfileAsync("ValidUser")).ReturnsAsync(validUserProfile);
            mock.Setup(api => api.GetUserProfileAsync("InvalidUser")).ReturnsAsync(nullUserProfile);

            mock.Setup(api => api.GetUserProfileAsync(validUser)).ReturnsAsync(validUserProfile);
            mock.Setup(api => api.GetUserProfileAsync(nullUser)).ReturnsAsync(nullUserProfile);

            mock.Setup(api => api.UpdateUserProfileAsync(validUser, It.Is<Dictionary<string, string>>(i => i.ContainsKey("BioText")))).ReturnsAsync(updatedUserProfile);
            mock.Setup(api => api.UpdateUserProfileAsync(validUser, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateUserProfileAsync(validUserProfile, It.Is<Dictionary<string, string>>(i => i.ContainsKey("BioText")))).ReturnsAsync(updatedUserProfile);
            mock.Setup(api => api.UpdateUserProfileAsync(validUserProfile, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateUserProfileAsync((User)null, It.IsAny<Dictionary<string, string>>())).Throws<ArgumentNullException>();
            mock.Setup(api => api.UpdateUserProfileAsync(validUser, null)).Throws<ArgumentNullException>();

            api = mock.Object;
        }

        [TestMethod]
        public async Task GetUserProfile_ByName()
        {
            UserProfile userProfile = await api.GetUserProfileAsync("ValidUser");
            Assert.IsNotNull(userProfile);
            Assert.AreEqual("Valid bio", userProfile.BioText);
            Assert.AreEqual("Valid User", userProfile.DisplayName);
        }

        [TestMethod]
        public async Task GetUserProfile_ByName_Invalid()
        {
            UserProfile userProfile = await api.GetUserProfileAsync("InvalidUser");
            Assert.IsNull(userProfile);
        }

        [TestMethod]
        public async Task GetUserProfile_ByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            UserProfile userProfile = await api.GetUserProfileAsync(user);
            Assert.IsNotNull(userProfile);
            Assert.AreEqual("Valid bio", userProfile.BioText);
            Assert.AreEqual("Valid User", userProfile.DisplayName);
        }

        [TestMethod]
        public async Task GetUserProfile_ByUser_Invalid()
        {
            User user = await api.GetUserAsync("InvalidUser");
            UserProfile userProfile = await api.GetUserProfileAsync(user);
            Assert.IsNull(userProfile);
        }

        [TestMethod]
        public async Task UpdateUserProfile()
        {
            User user = await api.GetUserAsync("ValidUser");
            UserProfile userProfile = await api.GetUserProfileAsync(user);
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "BioText", "Updated bio" } };
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync(userProfile, updates);
            Assert.IsNotNull(updatedUserProfile);
            Assert.AreEqual("Updated bio", updatedUserProfile.BioText);
            Assert.AreEqual("Valid User", updatedUserProfile.DisplayName);
        }

        [TestMethod]
        public async Task UpdateUserProfile_ByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "BioText", "Updated bio" } };
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync(user, updates);
            Assert.IsNotNull(updatedUserProfile);
            Assert.AreEqual("Updated bio", updatedUserProfile.BioText);
            Assert.AreEqual("Valid User", updatedUserProfile.DisplayName);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateUserProfile_InvalidUser()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync((User)null, updates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateUserProfile_InvalidArguments()
        {
            User user = await api.GetUserAsync("InvalidUser");
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync(user, null);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateUserProfile_InvalidUser_ByUser()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync(user, updates);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task UpdateUserProfile_Invalid()
        {
            User user = await api.GetUserAsync("ValidUser");
            UserProfile userProfile = await api.GetUserProfileAsync(user);
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync(userProfile, updates);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task UpdateUserProfile_Invalid_ByUser()
        {
            User user = await api.GetUserAsync("ValidUser");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            UserProfile updatedUserProfile = await api.UpdateUserProfileAsync(user, updates);
        }
    }
}
