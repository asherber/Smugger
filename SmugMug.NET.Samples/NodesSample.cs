using SmugMug;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmugMug.NET.Samples
{
    class NodesSample
    {
        public static async Task WorkingWithNodes(SmugMugClient api)
        {
            //Get access to the user you want to enumerate albums for
            User user = await api.GetUserAsync("cmac");
            Console.WriteLine(user.Name);

            //Get the root node for the given user
            Node rootNode = await api.GetRootNodeAsync(user);
            Console.WriteLine(rootNode);

            //Get the children of the root node
            List<Node> childNodes = await api.GetChildNodesAsync(rootNode);
            Console.WriteLine("The first node '{0}' is a {1}", childNodes[0].Name, childNodes[0].Type);

            //Get a specific node, "XWx8t"
            Node node = await api.GetNodeAsync("XWx8t");
            Console.WriteLine("Node '{0}' is a {1}", node.Name, node.Type);
        }

        public static async Task ManagingNodes(SmugMugClient api)
        {
            //Get access to the logged in user
            User user = await api.GetAuthenticatedUserAsync();
            Console.WriteLine("{0} is currently authenticated", user.Name);

            //Get the root node ID
            string defaultNodeID = api.GetDefaultNodeIDAsync(user);

            //Create a new folder node at the root
            Node folderNode = await api.CreateNodeAsync(NodeType.Folder, "TestFolderNode", defaultNodeID);
            Console.WriteLine("Created folder node {0}", folderNode.Name);

            //Create a new album node in that folder
            Dictionary<string, string> arguments = new Dictionary<string, string>() { { "Description", "test description" } };
            Node albumNode = await api.CreateNodeAsync(NodeType.Album, "TestAlbumNode", folderNode.NodeID, arguments);
            Console.WriteLine("Created album node {0} {1}", albumNode.Name, albumNode.Description);

            Dictionary<string, string> albumUpdates = new Dictionary<string, string>() { { "Name", "Updated Album Name" }, { "Description", "Updated description" }, { "SortDirection", "Ascending" } };
            Node updatedAlbumNode = await api.UpdateNodeAsync(albumNode, albumUpdates);
            Console.WriteLine("Updated album node {0}: {1}", updatedAlbumNode.Name, updatedAlbumNode.Description);

            //Delete the newly created nodes
            await api.DeleteNodeAsync(folderNode);
        }
    }
}