using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SmugMug.NET.Tests
{
    [TestClass]
    public class NodeUnitTests
    {
        private ISmugMugClient api;

        [TestInitialize()]
        public void InitializeAnonymous()
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

            mock.Setup(api => api.GetChildNodesAsync(validNode, It.IsInRange<int>(0, int.MaxValue, Range.Inclusive))).ReturnsAsync(validNodes);
            mock.Setup(api => api.GetChildNodesAsync(validNode, It.IsInRange<int>(int.MinValue, 0, Range.Inclusive))).ReturnsAsync(invalidNodes);
            mock.Setup(api => api.GetChildNodesAsync(invalidNode, It.IsInRange<int>(0, int.MaxValue, Range.Inclusive))).ReturnsAsync(invalidNodes);
            mock.Setup(api => api.GetChildNodesAsync(invalidNode, It.IsInRange<int>(int.MinValue, 0, Range.Inclusive))).ReturnsAsync(invalidNodes);
            mock.Setup(api => api.GetChildNodesAsync(validNodeNoChildren, It.IsInRange<int>(int.MinValue, int.MaxValue, Range.Inclusive))).ReturnsAsync(invalidNodes);

            mock.Setup(api => api.DeleteNodeAsync(invalidNode)).Throws<ArgumentNullException>();
            mock.Setup(api => api.DeleteNodeAsync(unownedNode)).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateNodeAsync(validNode, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Name")))).ReturnsAsync(updatedNode);
            mock.Setup(api => api.UpdateNodeAsync(validNode, It.Is<Dictionary<string, string>>(i => i.ContainsKey("Invalid")))).Throws<HttpRequestException>();

            mock.Setup(api => api.UpdateNodeAsync((Node)null, It.IsAny<Dictionary<string, string>>())).Throws<ArgumentNullException>();
            mock.Setup(api => api.UpdateNodeAsync(validNode, null)).Throws<ArgumentNullException>();

            api = mock.Object;
        }

        [TestMethod]
        public async Task GetNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Assert.IsNotNull(node);
            Assert.AreEqual("ValidNode", node.Name);
            Assert.AreEqual("ABCDE", node.NodeID);
        }
        
        [TestMethod]
        public async Task GetNode_Invalid()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            Assert.IsNull(node);
        }

        [TestMethod]
        public async Task GetNode_Unowned()
        {
            Node node = await api.GetNodeAsync("Unowned`Node");
            Assert.IsNull(node);
        }

        [TestMethod]
        public async Task GetRootNode()
        {
            User user = await api.GetUserAsync("ValidUser");
            Node node = await api.GetRootNodeAsync(user);
            Assert.IsNotNull(node);
            Assert.AreEqual("ValidNode", node.Name);
            Assert.AreEqual("ABCDE", node.NodeID);
        }

        [TestMethod]
        public async Task GetRootNode_Invalid()
        {
            User user = await api.GetUserAsync("InvalidUser");
            Node node = await api.GetRootNodeAsync(user);
            Assert.IsNull(node);
        }

        [TestMethod]
        public async Task GetChildNodes()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node);
            Assert.IsNotNull(childNodes);
            Assert.IsTrue(childNodes.Count > 0);
        }

        [TestMethod]
        public async Task GetChildNodes_InvalidNode()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node);
            Assert.IsNull(childNodes);
        }

        [TestMethod]
        public async Task GetChildNodes_NoChildren()
        {
            Node node = await api.GetNodeAsync("ValidNodeNoChildren");
            List<Node> childNodes = await api.GetChildNodesAsync(node);
            Assert.IsNull(childNodes);
        }

        [TestMethod]
        public async Task GetChildNodes_withLimit()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node, 3);
            Assert.IsNotNull(childNodes);
            Assert.AreEqual(3, childNodes.Count);
        }

        [TestMethod]
        public async Task GetChildNodes_withInvalidLimit()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            List<Node> childNodes = await api.GetChildNodesAsync(node, -1);
            Assert.IsNull(childNodes);
        }

        [TestMethod]
        public async Task CreateNode_Album_NoArguments()
        {
            Node node = await api.CreateNodeAsync(NodeType.Album, "AlbumNode", "ValidPath");
            Assert.IsNotNull(node);
            Assert.AreEqual(NodeType.Album, node.Type);
            Assert.AreEqual("AlbumNode", node.Name);
        }

        [TestMethod]
        public async Task CreateNode_Folder_NoArguments()
        {
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "ValidPath", new Dictionary<string, string>()); //TODO: Should be null for final argument
            Assert.IsNotNull(node);
            Assert.AreEqual(NodeType.Folder, node.Type);
            Assert.AreEqual("FolderNode", node.Name);
            Assert.IsNull(node.Description);
        }

        [TestMethod]
        public async Task CreateNode_Folder_WithArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "Description" } };
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "ValidPath", arguments);
            Assert.IsNotNull(node);
            Assert.AreEqual(NodeType.Folder, node.Type);
            Assert.AreEqual("FolderNode", node.Name);
            Assert.AreEqual("Description", node.Description);
        }

        [TestMethod]
        public async Task CreateNode_Page_NoArguments()
        {
            Node node = await api.CreateNodeAsync(NodeType.Page, "PageNode", "ValidPath");
            Assert.IsNotNull(node);
            Assert.AreEqual(NodeType.Page, node.Type);
            Assert.AreEqual("PageNode", node.Name);
        }

        [TestMethod]
        public async Task CreateNode_InvalidPath()
        {
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "InvalidPath");
            Assert.IsNull(node);
        }

        [TestMethod]
        public async Task CreateNode_Folder_WithInvalidArguments()
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Node node = await api.CreateNodeAsync(NodeType.Folder, "FolderNode", "ValidPath", arguments);
            Assert.IsNull(node);
        }

        [TestMethod]
        public async Task DeleteNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            await api.DeleteNodeAsync(node);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteNode_Invalid()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            await api.DeleteNodeAsync(node);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task DeleteNode_Unowned()
        {
            Node node = await api.GetNodeAsync("UnownedNode");
            await api.DeleteNodeAsync(node);
        }

        [TestMethod]
        public async Task UpdateNode()
        {
            Node node = await api.GetNodeAsync("ValidNode");

            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Name", "Updated node" } };

            Node updatedNode = await api.UpdateNodeAsync(node, updates);
            Assert.IsNotNull(updatedNode);
            Assert.AreEqual("Updated node", updatedNode.Name);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateNode_InvalidNode()
        {
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Node updatedNode = await api.UpdateNodeAsync((Node)null, updates);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task UpdateNode_InvalidNodeNullArguments()
        {
            Node node = await api.GetNodeAsync("InvalidNode");
            Node updatedNode = await api.UpdateNodeAsync(node, null);
        }

        [TestMethod]
        [ExpectedException(typeof(HttpRequestException))]
        public async Task UpdateNode_InvalidArguments()
        {
            Node node = await api.GetNodeAsync("ValidNode");
            Dictionary<string, string> updates = new Dictionary<string, string>() { { "Invalid", "Invalid" } };
            Node updatedNode = await api.UpdateNodeAsync(node, updates);
        }
    }
}
