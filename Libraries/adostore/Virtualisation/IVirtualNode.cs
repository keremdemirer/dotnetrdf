﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage.Virtualisation
{
    public interface IVirtualNode<TNodeID, TGraphID> : INode
    {
        TNodeID VirtualID
        {
            get;
        }

        IVirtualRdfProvider<TNodeID, TGraphID> Provider
        {
            get;
        }

        bool IsMaterialised
        {
            get;
        }

        INode MaterialisedValue
        {
            get;
        }
    }
}