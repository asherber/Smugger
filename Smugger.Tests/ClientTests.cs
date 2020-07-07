using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using FluentAssertions.Formatting;
using System.Runtime.InteropServices;

namespace Smugger.Tests
{
    public class ClientTests
    {
        public static IEnumerable<object[]> EmptyApiKeys => new[]
        {
            new [] { (string) null },
            new [] { String.Empty }
        };

        const string VALUE = "someValue";

        public static IEnumerable<object[]> EmptyCreds => new[]
        {
            new [] { null, VALUE, VALUE, VALUE, "consumerKey" },
            new [] { String.Empty, VALUE, VALUE, VALUE, "consumerKey" },

            new [] { VALUE, null, VALUE, VALUE ,"consumerSecret" },
            new [] { VALUE, String.Empty, VALUE, VALUE, "consumerSecret" },

            new [] { VALUE, VALUE, null, VALUE, "accessToken" },
            new [] { VALUE, VALUE, String.Empty, VALUE, "accessToken" },

            new [] { VALUE, VALUE, VALUE, null, "accessTokenSecret" },
            new [] { VALUE, VALUE, VALUE, String.Empty, "accessTokenSecret" },
        };

        [Theory]
        [MemberData(nameof(EmptyApiKeys))]
        public void Create_No_ApiKey(string input)
        {
            Action act = () => new SmugMugClient(input);
            act.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("apiKey");
        }

        [Theory]
        [MemberData(nameof(EmptyCreds))]
        public void Create_Missing_Param(string cKey, string cSecret, string aToken, string aSecret, string parm)
        {
            Action act = () => new SmugMugClient(cKey, cSecret, aToken, aSecret);
            act.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be(parm);
        }

        [Fact]
        public void Create_Null_Creds()
        {
            OAuthCredentials input = null;
            Action act = () => new SmugMugClient(input);
            act.Should().Throw<ArgumentNullException>()
                .Which.ParamName.Should().Be("credentials");
        }

        [Fact]
        public void Create_Anonymous()
        {
            var input = new SmugMugClient("abc");
            input.LoginType.Should().Be(LoginType.Anonymous);
        }

        [Fact]
        public void Create_OAuth()
        {
            var input = new SmugMugClient("a", "b", "c", "d");
            input.LoginType.Should().Be(LoginType.OAuth);
        }
    }
}
