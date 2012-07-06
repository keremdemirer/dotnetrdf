﻿/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;

namespace VDS.RDF.Storage
{
    /// <summary>
    /// Abstract Base Class for <see cref="IStorageProvider">IStorageProvider</see> implementations for which it is safe to do the <see cref="IAsyncStorageProvider">IAsyncStorageProvider</see> implementation simply by farming out calls to the synchronous methods onto background threads (i.e. non-HTTP based connectors)
    /// </summary>
    public abstract class BaseAsyncSafeConnector
        : IStorageProvider, IGenericIOManager, IAsyncStorageProvider
    {
        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public abstract void LoadGraph(IGraph g, Uri graphUri);

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public abstract void LoadGraph(IGraph g, string graphUri);

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public abstract void LoadGraph(IRdfHandler handler, Uri graphUri);

        /// <summary>
        /// Loads a Graph from the Store
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        public abstract void LoadGraph(IRdfHandler handler, string graphUri);

        /// <summary>
        /// Saves a Graph to the Store
        /// </summary>
        /// <param name="g">Graph to save</param>
        public abstract void SaveGraph(IGraph g);

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public abstract void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        /// <summary>
        /// Updates a Graph in the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        public abstract void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals);

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public abstract void DeleteGraph(Uri graphUri);

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        public abstract void DeleteGraph(string graphUri);

        /// <summary>
        /// Lists the Graphs in the Store
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Uri> ListGraphs();

        /// <summary>
        /// Indicates whether the Store is ready to accept requests
        /// </summary>
        public abstract bool IsReady
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store is read only
        /// </summary>
        public abstract bool IsReadOnly
        {
            get;
        }

        /// <summary>
        /// Gets the IO Behaviour of the Store
        /// </summary>
        public abstract IOBehaviour IOBehaviour
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports Triple level updates via the <see cref="BaseAsyncSafeConnector.UpdateGraph">UpdateGraph()</see> method
        /// </summary>
        public abstract bool UpdateSupported
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports Graph deletion via the <see cref="BaseAsyncSafeConnector.DeleteGraph">DeleteGraph()</see> method
        /// </summary>
        public abstract bool DeleteSupported
        {
            get;
        }

        /// <summary>
        /// Gets whether the Store supports listing graphs via the <see cref="BaseAsyncSafeConnector.ListGraphs">ListGraphs()</see> method
        /// </summary>
        public abstract bool ListGraphsSupported
        {
            get;
        }

        /// <summary>
        /// Diposes of the Store
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void LoadGraph(IGraph g, Uri graphUri, AsyncStorageCallback callback, object state)
        {
            this.AsyncLoadGraph(g, graphUri, callback, state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to load into</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void LoadGraph(IGraph g, string graphUri, AsyncStorageCallback callback, object state)
        {
            Uri u = (String.IsNullOrEmpty(graphUri) ? null : UriFactory.Create(graphUri));
            this.LoadGraph(g, u, callback, state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void LoadGraph(IRdfHandler handler, Uri graphUri, AsyncStorageCallback callback, object state)
        {
            this.AsyncLoadGraph(handler, graphUri, callback, state);
        }

        /// <summary>
        /// Loads a Graph from the Store asynchronously
        /// </summary>
        /// <param name="handler">Handler to load with</param>
        /// <param name="graphUri">URI of the Graph to load</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void LoadGraph(IRdfHandler handler, string graphUri, AsyncStorageCallback callback, object state)
        {
            Uri u = (String.IsNullOrEmpty(graphUri) ? null : UriFactory.Create(graphUri));
            this.LoadGraph(handler, u, callback, state);
        }

        /// <summary>
        /// Saves a Graph to the Store asynchronously
        /// </summary>
        /// <param name="g">Graph to save</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void SaveGraph(IGraph g, AsyncStorageCallback callback, object state)
        {
            this.AsyncSaveGraph(g, callback, state);
        }

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void UpdateGraph(Uri graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            this.AsyncUpdateGraph(graphUri, additions, removals, callback, state);
        }

        /// <summary>
        /// Updates a Graph in the Store asychronously
        /// </summary>
        /// <param name="graphUri">URI of the Graph to update</param>
        /// <param name="additions">Triples to be added</param>
        /// <param name="removals">Triples to be removed</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void UpdateGraph(string graphUri, IEnumerable<Triple> additions, IEnumerable<Triple> removals, AsyncStorageCallback callback, object state)
        {
            Uri u = (String.IsNullOrEmpty(graphUri) ? null : UriFactory.Create(graphUri));
            this.UpdateGraph(u, additions, removals, callback, state);
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void DeleteGraph(Uri graphUri, AsyncStorageCallback callback, object state)
        {
            this.AsyncDeleteGraph(graphUri, callback, state);
        }

        /// <summary>
        /// Deletes a Graph from the Store
        /// </summary>
        /// <param name="graphUri">URI of the Graph to delete</param>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void DeleteGraph(string graphUri, AsyncStorageCallback callback, object state)
        {
            Uri u = (String.IsNullOrEmpty(graphUri) ? null : UriFactory.Create(graphUri));
            this.DeleteGraph(u, callback, state);
        }

        /// <summary>
        /// Lists the Graphs in the Store asynchronously
        /// </summary>
        /// <param name="callback">Callback</param>
        /// <param name="state">State to pass to the callback</param>
        public void ListGraphs(AsyncStorageCallback callback, object state)
        {
            this.AsyncListGraphs(callback, state);
        }
    }
}