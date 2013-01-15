/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
#if !NO_WEB
using System.Web;
#endif

namespace VDS.RDF.Update
{
    /// <summary>
    /// A Class for connecting to a remote SPARQL Update endpoint and executing Updates against it
    /// </summary>
    public class SparqlRemoteUpdateEndpoint : BaseEndpoint
    {
        const int LongUpdateLength = 2048;

        /// <summary>
        /// Creates a new SPARQL Update Endpoint for the given URI
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public SparqlRemoteUpdateEndpoint(Uri endpointUri)
            : base(endpointUri) 
        {
            this.HttpMode = "POST";
        }

        /// <summary>
        /// Creates a new SPARQL Update Endpoint for the given URI
        /// </summary>
        /// <param name="endpointUri">Endpoint URI</param>
        public SparqlRemoteUpdateEndpoint(String endpointUri)
            : this(UriFactory.Create(endpointUri)) { }

        /// <summary>
        /// Gets/Sets the HTTP Method used for requests
        /// </summary>
        /// <remarks>
        /// The SPARQL 1.1 Protocol specification mandates that Update requests may only be POSTed, attempting to alter the HTTP Mode to anything other than POST will result in a <see cref="SparqlUpdateException">SparqlUpdateException</see>
        /// </remarks>
        public override string HttpMode
        {
            get
            {
                return base.HttpMode;
            }
            set
            {
                if (value.Equals("POST"))
                {
                    base.HttpMode = value;
                }
                else
                {
                    throw new SparqlUpdateException("The SPARQL 1.1 Protocol specification requires SPARQL Updates to be POSTed, you cannot set the HTTP Method to a value other than POST");
                }
            }
        }

#if !SILVERLIGHT

        /// <summary>
        /// Makes an update request to the remote endpoint
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        public void Update(String sparqlUpdate)
        {
            try
            {
                //Build the Request URI and POST Data
                StringBuilder requestUri = new StringBuilder();
                requestUri.Append(this.Uri.AbsoluteUri);
                StringBuilder postData = new StringBuilder();
                bool longUpdate = false;
                if (!this.HttpMode.Equals("POST") && sparqlUpdate.Length <= LongUpdateLength)
                {
                    if (!this.Uri.Query.Equals(String.Empty))
                    {
                        requestUri.Append("&update=");
                    }
                    else
                    {
                        requestUri.Append("?update=");
                    }
                    requestUri.Append(HttpUtility.UrlEncode(sparqlUpdate));
                }
                else
                {
                    longUpdate = true;
                    postData.Append("update=");
                    postData.Append(HttpUtility.UrlEncode(sparqlUpdate));
                }

                //Make the request
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri.ToString());

                //Apply Credentials to request if necessary
                if (this.Credentials != null)
                {
                    request.Credentials = this.Credentials;
                }
                request.PreAuthenticate = true;

#if !NO_PROXY
                //Use a Proxy if required
                if (this.Proxy != null)
                {
                    request.Proxy = this.Proxy;
                    if (this.UseCredentialsForProxy)
                    {
                        request.Proxy.Credentials = this.Credentials;
                    }
                }
#endif

#if DEBUG
                if (Options.HttpDebugging)
                {
                    Tools.HttpDebugRequest(request);
                }
#endif
                if (longUpdate)
                {
                    request.Method = "POST";
                    request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
                    using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                    {
                        writer.Write(postData);
                        writer.Close();
                    }
                }
                else
                {
                    request.Method = this.HttpMode;
                }
                request.Accept = MimeTypesHelper.Any;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
#if DEBUG
                    if (Options.HttpDebugging)
                    {
                        Tools.HttpDebugResponse(response);
                    }
#endif
                    //If we don't get an error then we should be fine
                    response.Close();
                }

            }
            catch (WebException webEx)
            {
#if DEBUG
                if (Options.HttpDebugging)
                {
                    if (webEx.Response != null) Tools.HttpDebugResponse((HttpWebResponse)webEx.Response);
                }
#endif
                //Some sort of HTTP Error occurred
                throw new SparqlUpdateException("A HTTP Error occurred when trying to make the SPARQL Update", webEx);
            }
        }

#endif

        /// <summary>
        /// Makes an update request asynchronously to the remote endpoint
        /// </summary>
        /// <param name="sparqlUpdate">SPARQL Update</param>
        /// <param name="callback">Callback to invoke when the update completes</param>
        /// <param name="state">State to pass to the callback</param>
        public void Update(String sparqlUpdate, UpdateCallback callback, Object state)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(this.Uri);
            request.Method = "POST";
            request.ContentType = MimeTypesHelper.WWWFormURLEncoded;
            request.Accept = MimeTypesHelper.Any;

            //Apply Credentials to request if necessary
            if (this.Credentials != null)
            {
                request.Credentials = this.Credentials;
            }
#if !SILVERLIGHT
            request.PreAuthenticate = true;
#endif

#if !NO_PROXY
            //Use a Proxy if required
            if (this.Proxy != null)
            {
                request.Proxy = this.Proxy;
                if (this.UseCredentialsForProxy)
                {
                    request.Proxy.Credentials = this.Credentials;
                }
            }
#endif

#if DEBUG
            if (Options.HttpDebugging)
            {
                Tools.HttpDebugRequest(request);
            }
#endif

            request.BeginGetRequestStream(result =>
                {
                    Stream stream = request.EndGetRequestStream(result);
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write("update=");
                        writer.Write(HttpUtility.UrlEncode(sparqlUpdate));

                        writer.Close();
                    }

                    request.BeginGetResponse(innerResult =>
                        {
                            using (HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(innerResult))
                            {
#if DEBUG
                                if (Options.HttpDebugging)
                                {
                                    Tools.HttpDebugResponse(response);
                                }
#endif

                                response.Close();
                                callback(state);
                            }
                        }, null);
                }, null);
        }
    }
}
