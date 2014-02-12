using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GEDs.Helpers
{
    public class Component
    {
        private string sequence;
        public string Sequence
        {
            get
            {
                if (sequence == null)
                    return string.Empty;
                return sequence;
            }
            set
            {
                sequence = value;
            }
        }

        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                    return string.Empty;
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string title;
        public string Title
        {
            get
            {
                if (title == null)
                    return string.Empty;
                return title;
            }
            set
            {
                title = value;
            }
        }

        private string phone;
        public string Phone
        {
            get
            {
                if (phone == null)
                    return string.Empty;
                return phone;
            }
            set
            {
                phone = value;
            }
        }

        private string pguid;
        public string PGUID
        {
            get
            {
                if (pguid == null)
                    return string.Empty;
                return pguid;
            }
            set
            {
                pguid = value;
            }
        }

        private string address;
        public string Address
        {
            get
            {
                if (address == null)
                    return string.Empty;
                return address;
            }
            set
            {
                address = value;
            }
        }

        private string city;
        public string City
        {
            get
            {
                if (city == null)
                    return string.Empty;
                return city;
            }
            set
            {
                city = value;
            }
        }

        private string province;
        public string Province
        {
            get
            {
                if (province == null)
                    return string.Empty;
                return province;
            }
            set
            {
                province = value;
            }
        }

        private string postal;
        public string PostalCode
        {
            get
            {
                if (postal == null)
                    return string.Empty;
                return postal;
            }
            set
            {
                postal = value;
            }
        }

        private string country;
        public string Country
        {
            get
            {
                if (country == null)
                    return string.Empty;
                return country;
            }
            set
            {
                country = value;
            }
        }
    }

    public class Structure
    {
        private string sequence;
        public string Sequence
        {
            get
            {
                if (sequence == null)
                    return string.Empty;
                return sequence;
            }
            set
            {
                sequence = value;
            }
        }

        private string name;
        public string Name
        {
            get
            {
                if (name == null)
                    return string.Empty;
                return name;
            }
            set
            {
                name = value;
            }
        }

        private string fname;
        public string FName
        {
            get
            {
                if (fname == null)
                    return string.Empty;
                return fname;
            }
            set
            {
                fname = value;
            }
        }

        private string guid;
        public string GUID
        {
            get
            {
                if (guid == null)
                    return string.Empty;
                return guid;
            }
            set
            {
                guid = value;
            }
        }

        private string pguid;
        public string PGUID
        {
            get
            {
                if (pguid == null)
                    return string.Empty;
                return pguid;
            }
            set
            {
                pguid = value;
            }
        }
    }

    public class TreeNode<T>
    {
        private readonly T _value;
        private readonly List<TreeNode<T>> _children = new List<TreeNode<T>>();

        public TreeNode(T value)
        {
            _value = value;
        }

        public TreeNode<T> this[int i]
        {
            get { return _children[i]; }
        }

        public TreeNode<T> Parent { get; private set; }

        public T Value { get { return _value; } }

        public List<TreeNode<T>> Children
        {
            get { return _children; }
        }

        public TreeNode<T> AddChild(T value)
        {
            var node = new TreeNode<T>(value) { Parent = this };
            _children.Add(node);
            return node;
        }

        public TreeNode<T>[] AddChildren(params T[] values)
        {
            return values.Select(AddChild).ToArray();
        }

        public bool RemoveChild(TreeNode<T> node)
        {
            return _children.Remove(node);
        }

        public void Traverse(Action<T> action)
        {
            action(Value);
            foreach (var child in _children)
                child.Traverse(action);
        }

        public TreeNode<T> Find(object node)
        {
            return Find(node, this);
        }

        private TreeNode<T> Find(object node, TreeNode<T> tree)
        {
            if (this.Value.Equals(node))
                return tree;

            foreach (var child in _children)
                return Find(node, child);

            return null;
        }

        public IEnumerable<TreeNode<T>> Descendants()
        {
            var nodes = new Stack<TreeNode<T>>(new[] { this });
            while (nodes.Any())
            {
                TreeNode<T> node = nodes.Pop();
                yield return node;
                foreach (var n in node._children) nodes.Push(n);
            }
        }

        public IEnumerable<T> Flatten()
        {
            return new[] { Value }.Union(_children.SelectMany(x => x.Flatten()));
        }
    }

    public class Node
    {
        private Structure structure;
        private List<Component> components;

        public Structure Structure
        {
            get
            {
                if (structure == null)
                    return new Structure();
                return structure;
            }
            set
            {
                structure = value;
            }
        }

        public List<Component> Components
        {
            get
            {
                if (components == null)
                    return new List<Component>();
                return components;
            }
            set
            {
                components = null;
            }
        }

        public Node()
        {
            structure = new Structure();
            components = new List<Component>();
        }

        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(obj.GetType(), typeof(string)))
            {
                if (this.Structure.GUID == (string)obj)
                    return true;
                return false;
            }
            else if (Object.ReferenceEquals(obj.GetType(), typeof(Structure)))
            {
                if (this.Structure.GUID == ((Structure)obj).GUID)
                    return true;
                return false;
            }

            return false;
        }
    }


}