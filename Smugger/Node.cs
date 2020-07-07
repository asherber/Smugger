using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Smugger
{
    public class Node : SmugMugObject
    {
        public bool AutoRename { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateModified { get; set; }
        public string Description { get; set; }
        public PrivacyType EffectivePrivacy { get; set; }
        public SecurityType EffectiveSecurityType { get; set; }
        public FormattedValues FormattedValues { get; set; } //TODO: These are not being read properly
        public bool HasChildren { get; set; }
        public bool HideOwner { get; set; }
        public string HighlightImageUri { get; set; }
        public bool IsRoot { get; set; }
        public string[] Keywords { get; set; }
        public string Name { get; set; }
        public string NodeID { get; set; }
        public string Password { get; set; }
        public string PasswordHint { get; set; }
        public PrivacyType Privacy { get; set; }
        public SecurityType SecurityType { get; set; }
        public SmugSearchable SmugSearchable { get; set; }
        public SortDirection SortDirection { get; set; }
        public int SortIndex { get; set; }
        public SortMethod SortMethod { get; set; }
        public NodeType Type { get; set; }
        public string UrlName { get; set; }
        public string UrlPath { get; set; }
        public WorldSearchable WorldSearchable { get; set; }
        public Uri WebUri { get; set; }
        public NodeUris Uris { get; set; }

        public override string ToString()
        {
            return string.Format("{0}: {1}, {2}", Type, Name, JsonConvert.SerializeObject(this));
        }
    }

    public class FormattedValues
    {
        public class Name
        {
            public string html { get; set; }
        }

        public class Description
        {
            public string html { get; set; }
            public string text { get; set; }
        }
    }

    public class NodeUris
    {
        public SmugMugUri Album { get; set; }
        public SmugMugUri ChildNodes { get; set; }
        public SmugMugUri FolderById { get; set; }
        public SmugMugUri HighlightImage { get; set; }
        public SmugMugUri MoveNodes { get; set; }
        public SmugMugUri NodeGrants { get; set; }
        public SmugMugUri ParentNode { get; set; }
        public SmugMugUri ParentNodes { get; set; }
        public SmugMugUri User { get; set; }
    }

    public class NodeGetResponse : SmugMugUri
    {
        public string DocUri { get; set; }
        public Node Node { get; set; }
    }

    public class NodePostResponse :  SmugMugUri
    {
        public Node Node { get; set; }

        public override string ToString()
        {
            return Node.ToString();
        }
    }

    public class NodePagesResponse : SmugMugPagesObject
    {
        public IEnumerable<Node> Node { get; set; }
    }
}