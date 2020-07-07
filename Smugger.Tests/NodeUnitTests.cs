using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Smugger.Tests
{
    public class NodeUnitTests
    {
        private ISmugMugClient api;

        public NodeUnitTests()
        {
            var mock = new Mock<ISmugMugClient>();

            Node invalidNode = null;
            Node validNode = new Node() { Name = "ValidNode", NodeID = "ABCDE", HasChildren = true };
            Node validNodeNoChildren = new Node() { Name = "ValidNode", NodeID = "ABCDE", HasChildren = false };
            Node unownedNode = new Node() { Name = "UnownedNode", NodeID = "ABCDE", HasChildren = true };

            mock.Setup(api => api.GetNodeAsync("ValidNode")).ReturnsAsync(validNode);
            mock.Setup(api => api.GetNodeAsync("InvalidNode")).ReturnsAsync(invalidNode);
            mock.Setup(api => api.GetNodeAsync("UnownedNode")).ReturnsAsync(unownedNode);
            mock.Setup(api => api.GetNodeAsync("ValidNodeNoChildren")).ReturnsAsync(validNodeNoChildren);

            User nullUser = null;
            User validUser = new User() { Name = "Valid User", NickName = "ValidUser" };

            mock.Setup(api => api.GetUserAsync("ValidUser")).ReturnsAsync(validUser);
            mock.Setup(api => api.GetUserAsync("InvalidUser")).ReturnsAsync(nullUser);

            mock.Setup(api => api.GetRootNodeAsync(validUser)).ReturnsAsync(validNode);
            mock.Setup(api => api.GetRootNodeAsync(nullUser)).ReturnsAsync(invalidNode);

            Node folderNode = new Node() { Type = NodeType.Folder, Name = "FolderNode", NodeID = "ABCDE" };
            Node folderNodeWithArugments = new Node() { Type = NodeType.Folder, Name = "FolderNode", NodeID = "ABCDE", Description="Description" };
            Node albumNode = new Node() { Type = NodeType.Album, Name = "AlbumNode", NodeID = "ABCDE" };
            Node pageNode = new Node() { Type = NodeType.Page, Name = "PageNode", NodeID = "ABCDE" };
            Node nullNode = null;
            Node updatedNode = new Node() { Type = NodeType.Folder, Name = "Updated node", NodeID = "ABCDE" };

            mock.Setup(api => api.CreateNodeAsync(NodeType.Folder, It.IsAny<string>(), "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).ReturnsAsync(nullNode);
            mock.Setup(api => api.CreateNodeAsync(NodeType.Folder, It.IsAny<string>(), "ValidPath", It.Is<Dictionary<string, string>>(i => i.ContainsKey("Description")))).ReturnsAsync(folderNodeWithArugments);
            mock.Setup(api => api.CreateNodeAsync(NodeType.Folder, It.IsAny<string>(), "ValidPath", It.Is<Dictionary<string, string>>(i => !i.ContainsKey("Invalid") && !i.ContainsKey("Description")))).ReturnsAsync(folderNode);
            mock.Setup(api => api.CreateNodeAsync(NodeType.Folder, It.IsAny<string>(), "InvalidPath", It.IsAny<Dictionary<string, string>>())).ReturnsAsync(nullNode);

            mock.Setup(api => api.CreateNodeAsync(NodeType.Album, It.IsAny<string>(), "ValidPath", null)).ReturnsAsync(albumNode);
            mock.Setup(api => api.CreateNodeAsync(NodeType.Album, It.IsAny<string>(), "InvalidPath", It.IsAny<Dictionary<string, string>>())).ReturnsAsync(nullNode);

            mock.Setup(api => api.CreateNodeAsync(NodeType.Page, It.IsAny<string>(), "ValidPath", null)).ReturnsAsync(pageNode);
            mock.Setup(api => api.CreateNodeAsync(NodeType.Page, It.IsAny<string>(), "InvalidPath", It.IsAny<Dictionary<string, string>>())).ReturnsAsync(nullNode);

            List<Node> validNodes = new List<Node>() { folderNode, albumNode, pageNode};
            List<Node> invalidNodes = null;

            mock.Setup(api => api.GetChildNodesAsync(validNode, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(validNodes);
            mock.Setup(api => api.GetChildNodesAsync(validNode, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidNodes);
            mock.Setup(api => api.GetChildNodesAsync(invalidNode, It.IsInRange<int>(0, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(invalidNodes);
            mock.Setup(api => api.GetChildNodesAsync(invalidNode, It.IsInRange<int>(int.MinValue, 0, Moq.Range.Inclusive))).ReturnsAsync(invalidNodes);
            mock.Setup(api => api.GetChildNodesAsync(validNodeNoChildren, It.IsInRange<int>(int.MinValue, int.MaxValue, Moq.Range.Inclusive))).ReturnsAsync(invalidNodes);

            mock.Setup(api => api.DeleteNodeAsync(invalidNode)).Throws<ArgumentNullException>();
            mock.Setup(api => api.DeleteNodeAsync(unownedNode)).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateNodeAsync(validNode, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Name")))).ReturnsAsync(updatedNode);
            mock.Setup(api => api.UpdateNodeAsync(validNode, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateNodeAsync((Node)null, It.IsAny<Dictionary<string, string>>())).Throws<ArgumentNullException>();
            mock.Setup(api => api.UpdateNodeAsync(validNode, null)).Throws<ArgumentNullException>();

            api = mock.Object;
        }

        [Fact]
        public async Task GetNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Assert.NotNull(node);
            Assert.Equal("ValidNode", node.Name);
            Assert.Equal("ABCDE", node.NodeID);
        }
        
        [Fact]
        public async Task GetNode_Invalid()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            Assert.Null(node);
        }

        [Fact]
        public async Task GetNode_Unowned()
        {
            Node node = await api.GetNodeAsync("Unowned`Node");
            Assert.Null(node);
        }

        [Fact]
        public async Task GetRootNode()
        {
            User user = await api.GetUserAsync("ValidUser");
            Node node = await api.GetRootNodeAsync(user);
            Assert.NotNull(node);
            Assert.Equal("ValidNode", node.Name);
            Assert.Equal("ABCDE", node.NodeID);
        }

        [Fact]
        public async Task GetRootNode_Invalid()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Node node = await api.GetRootNodeAsync(user);
            Assert.Null(node);
        }

        [Fact]
        public async Task GetChildNodes()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node);
            Assert.NotNull(childNodes);
            Assert.True(childNodes.Count > 0);
        }

        [Fact]
        public async Task GetChildNodes_InvalidNode()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node);
            Assert.Null(childNodes);
        }

        [Fact]
        public async Task GetChildNodes_NoChildren()
        {
            Node node = await api.GetNodeAsync("ValidNodeNoChildren");
            List<Node> childNodes = await api.GetChildNodesAsync(node);
            Assert.Null(childNodes);
        }

        [Fact]
        public async Task GetChildNodes_withLimit()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node, 3);
            Assert.NotNull(childNodes);
            Assert.Equal(3, childNodes.Count);
        }

        [Fact]
        public async Task GetChildNodes_withInvalidLimit()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node, -1);
            Assert.Null(childNodes);
        }

        [Fact]
        public async Task CreateNode_Album_NoArguments()
        {
            Node node = await api.CreateNodeAsync(NodeType.Album, "AlbumNode", "ValidPath");
            Assert.NotNull(node);
            Assert.Equal(NodeType.Album, node.Type);
            Assert.Equal("AlbumNode", node.Name);
        }

        [Fact]
        public async Task CreateNode_Folder_NoArguments()
        {
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.NotNull(node);
            Assert.Equal(NodeType.Folder, node.Type);
            Assert.Equal("FolderNode", node.Name);
            Assert.Null(node.Description);
        }

        [Fact]
        public async Task CreateNode_Folder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "ValidPath", arguments);
            Assert.NotNull(node);
            Assert.Equal(NodeType.Folder, node.Type);
            Assert.Equal("FolderNode", node.Name);
            Assert.Equal("Description", node.Description);
        }

        [Fact]
        public async Task CreateNode_Page_NoArguments()
        {
            Node node = await api.CreateNodeAsync(NodeType.Page, "PageNode", "ValidPath");
            Assert.NotNull(node);
            Assert.Equal(NodeType.Page, node.Type);
            Assert.Equal("PageNode", node.Name);
        }

        [Fact]
        public async Task CreateNode_InvalidPath()
        {
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "InvalidPath");
            Assert.Null(node);
        }

        [Fact]
        public async Task CreateNode_Folder_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "ValidPath", arguments);
            Assert.Null(node);
        }

        [Fact]
        public async Task DeleteNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            await api.DeleteNodeAsync(node);
        }

        [Fact]
        public async Task DeleteNode_Invalid()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.DeleteNodeAsync(node));
        }

        [Fact]
        public async Task DeleteNode_Unowned()
        {
            Node node = await api.GetNodeAsync("UnownedNode");
            await Assert.ThrowsAsync<HttpRequestException>(() => api.DeleteNodeAsync(node));
        }

        [Fact]
        public async Task UpdateNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Name", "Updated node" } };

            Node updatedNode = await api.UpdateNodeAsync(node, updates);
            Assert.NotNull(updatedNode);
            Assert.Equal("Updated node", updatedNode.Name);
        }

        [Fact]
        public async Task UpdateNode_InvalidNode()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UpdateNodeAsync((Node)null, updates));
        }

        [Fact]
        public async Task UpdateNode_InvalidNodeNullArguments()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            await Assert.ThrowsAsync<ArgumentNullException>(() => api.UpdateNodeAsync(node, null));
        }

        [Fact]
        public async Task UpdateNode_InvalidArguments()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            await Assert.ThrowsAsync<HttpRequestException>(() => api.UpdateNodeAsync(node, updates));
        }
    }
}
