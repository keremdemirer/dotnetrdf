﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract Class for Nodes, implements the two basic properties of the INode Interface
    /// </summary>
    public abstract class BaseNode : INode
    {
        /// <summary>
        /// Reference to the Graph that the Node belongs to
        /// </summary>
        protected IGraph _graph;
        /// <summary>
        /// Uri of the Graph that the Node belongs to
        /// </summary>
        protected Uri _graphUri;
        /// <summary>
        /// Node Type for the Node
        /// </summary>
        protected NodeType _nodetype = NodeType.Literal;
        /// <summary>
        /// Stores the computed Hash Code for this Node
        /// </summary>
        protected int _hashcode;

        private bool _collides = false;

        /// <summary>
        /// Base Constructor which instantiates the Graph reference, Graph Uri and Node Type of the Node
        /// </summary>
        /// <param name="g">Graph this Node is in</param>
        /// <param name="type">Node Type</param>
        public BaseNode(IGraph g, NodeType type)
        {
            this._graph = g;
            if (this._graph != null) this._graphUri = this._graph.BaseUri;
            this._nodetype = type;
        }

        /// <summary>
        /// Nodes have a Type
        /// </summary>
        public NodeType NodeType
        {
            get
            {
                return _nodetype;
            }
        }

        /// <summary>
        /// Nodes belong to a Graph
        /// </summary>
        public IGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Gets/Sets the Graph Uri of the Node
        /// </summary>
        public Uri GraphUri
        {
            get
            {
                return this._graphUri;
            }
            set
            {
                this._graphUri = value;
            }
        }

        /// <summary>
        /// Gets/Sets whether the Nodes Hash Code collides with other Nodes in the Node Collection it is contained within
        /// </summary>
        public bool Collides
        {
            get
            {
                return this._collides;
            }
            set
            {
                this._collides = value;
            }
        }

        /// <summary>
        /// Nodes must implement an Equals method
        /// </summary>
        /// <param name="obj">Object to compare against</param>
        /// <returns></returns>
        public abstract override bool Equals(object obj);

        /// <summary>
        /// Nodes must implement a ToString method
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Essential for the implementation of GetHashCode to work correctly, Nodes should generate a String representation that is 'unique' as far as that is possible.
        /// </para>
        /// <para>
        /// Any two Nodes which match via the Equals method (based on strict RDF Specification Equality) should produce the same String representation since Hash Codes are generated by calling GetHashCode on this String
        /// </para>
        /// </remarks>
        public abstract override string ToString();

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <returns></returns>
        public virtual String ToString(INodeFormatter formatter)
        {
            return formatter.Format(this);
        }

        /// <summary>
        /// Gets the String representation of the Node formatted with the given Node formatter
        /// </summary>
        /// <param name="formatter">Formatter</param>
        /// <param name="segment">Triple Segment</param>
        /// <returns></returns>
        public virtual String ToString(INodeFormatter formatter, TripleSegment segment)
        {
            return formatter.Format(this, segment);
        }

        /// <summary>
        /// Gets a Hash Code for a Node
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// Implemented by getting the Hash Code of the result of ToString for a Node prefixed with its Node Type, this is pre-computed for efficiency when a Node is created since Nodes are immutable.  See remarks on ToString for more detail.
        /// </para>
        /// <para>
        /// Since Hash Codes are based on a String representation there is no guarantee of uniqueness though the same Triple will always give the same Hash Code (on a given Platform - see the MSDN Documentation for <see cref="System.String.GetHashCode">System.String.GetHashCode()</see> for further details)
        /// </para>
        /// </remarks>
        public override int GetHashCode()
        {
            return this._hashcode;
        }

        /// <summary>
        /// The Equality operator is defined for Nodes
        /// </summary>
        /// <param name="a">First Node</param>
        /// <param name="b">Second Node</param>
        /// <returns>Whether the two Nodes are equal</returns>
        /// <remarks>Uses the Equals method to evaluate the result</remarks>
        public static bool operator ==(BaseNode a, BaseNode b)
        {
            if (((Object)a) == null)
            {
                if (((Object)b) == null) return true;
                return false;
            }
            else
            {
                return a.Equals(b);
            }
        }

        /// <summary>
        /// The Non-Equality operator is defined for Nodes
        /// </summary>
        /// <param name="a">First Node</param>
        /// <param name="b">Second Node</param>
        /// <returns>Whether the two Nodes are non-equal</returns>
        /// <remarks>Uses the Equals method to evaluate the result</remarks>
        public static bool operator !=(BaseNode a, BaseNode b)
        {
            if (((Object)a) == null)
            {
                if (((Object)b) == null) return false;
                return true;
            }
            else
            {
                return !a.Equals(b);
            }
        }

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>
        /// </remarks>
        public abstract int CompareTo(INode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>
        /// </remarks>
        public abstract int CompareTo(IBlankNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>
        /// </remarks>
        public abstract int CompareTo(IGraphLiteralNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>
        /// </remarks>
        public abstract int CompareTo(ILiteralNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>
        /// </remarks>
        public abstract int CompareTo(IUriNode other);

        /// <summary>
        /// Nodes must implement a CompareTo method to allow them to be Sorted
        /// </summary>
        /// <param name="other">Node to compare self to</param>
        /// <returns></returns>
        /// <remarks>
        /// Implementations should use the SPARQL Term Sort Order for ordering nodes (as opposed to value sort order).  Standard implementations of Node type specific comparisons can be found in <see cref="ComparisonHelper">ComparisonHelper</see>
        /// </remarks>
        public abstract int CompareTo(IVariableNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>
        /// </remarks>
        public abstract bool Equals(INode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>
        /// </remarks>
        public abstract bool Equals(IBlankNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>
        /// </remarks>
        public abstract bool Equals(IGraphLiteralNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>
        /// </remarks>
        public abstract bool Equals(ILiteralNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>
        /// </remarks>
        public abstract bool Equals(IUriNode other);

        /// <summary>
        /// Nodes must implement an Equals method so we can do type specific equality
        /// </summary>
        /// <param name="other">Node to check for equality</param>
        /// <returns></returns>
        /// <remarks>
        /// Nodes implementations are also required to implement an override of the non-generic Equals method.  Standard implementations of some equality comparisons can be found in <see cref="EqualityHelper">EqualityHelper</see>
        /// </remarks>
        public abstract bool Equals(IVariableNode other);

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException("This INode implementation does not support Serialization");
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException("This INode implementation does not support XML Serialization");
        }
    }
}
