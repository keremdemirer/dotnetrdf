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
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Diagnostics;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Tokens;

namespace VDS.RDF
{
    /// <summary>
    /// Tools class which contains a number of utility methods which are declared as static methods
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Checks whether a Uri is valid as a Base Uri for resolving Relative URIs against
        /// </summary>
        /// <param name="baseUri">Base Uri to test</param>
        /// <returns>True if the Base Uri can be used to resolve Relative URIs against</returns>
        /// <remarks>A Base Uri is valid if it is an absolute Uri and not using the mailto: scheme</remarks>
        public static bool IsValidBaseUri(Uri baseUri)
        {
            if (baseUri.Scheme.Equals("mailto"))
            {
                return false;
            }
            else
            {
                return baseUri.IsAbsoluteUri;
            }
        }

        /// <summary>
        /// Checks whether a URI Reference appears malformed and if so fixes it
        /// </summary>
        /// <param name="uriref">URI Reference</param>
        /// <returns></returns>
        static String FixMalformedUriStrings(String uriref)
        {
            if (uriref.StartsWith("file:/"))
            {
                //HACK: This is something of a Hack as a workaround to the issue that some systems may generate RDF which have technically malformed file:// scheme URIs in it
                //This is because *nix style filesystems use paths of the form /path/to/somewhere and some serializers will serialize such
                //a file path by just prepending file: when they should be prepending file://
                if (uriref.Length > 6)
                {
                    if (uriref[6] != '/')
                    {
                        return "file://" + uriref.Substring(5);
                    }
                }
                return uriref;
            }
            else
            {
                return uriref;
            }
        }

        /// <summary>
        /// Returns a URI with any Fragment ID removed from it
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        public static Uri StripUriFragment(Uri u)
        {
            if (u.Fragment.Equals(String.Empty))
            {
                return u;
            }
            else
            {
                String temp = u.AbsoluteUri;
                temp = temp.Substring(0, temp.Length - u.Fragment.Length);
                return UriFactory.Create(temp);
            }
        }

        /// <summary>
        /// Generic Helper Function which Resolves Uri References against a Base Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to resolve</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns>Resolved Uri as a String</returns>
        /// <exception cref="RdfParseException">RDF Parse Exception if the Uri cannot be resolved for a know reason</exception>
        /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed</exception>
        public static String ResolveUri(String uriref, String baseUri)
        {
            if (!baseUri.Equals(String.Empty))
            {
                if (uriref.Equals(String.Empty))
                {
                    //Empty Uri reference refers to the Base Uri
                    return UriFactory.Create(Tools.FixMalformedUriStrings(baseUri)).AbsoluteUri;
                }
                else
                {
                    //Resolve the Uri by combining the Absolute/Relative Uri with the in-scope Base Uri
                    Uri u = new Uri(Tools.FixMalformedUriStrings(uriref), UriKind.RelativeOrAbsolute);
                    if (u.IsAbsoluteUri) 
                    {
                        //Uri Reference is an Absolute Uri so no need to resolve against Base Uri
                        return u.AbsoluteUri;
                    } 
                    else 
                    {
                        Uri b = UriFactory.Create(Tools.FixMalformedUriStrings(baseUri));

                        //Check that the Base Uri is valid for resolving Relative URIs
                        //If the Uri Reference is a Fragment ID then Base Uri validity is irrelevant
                        //We have to use ToString() here because this is a Relative URI so AbsoluteUri would be invalid here
                        if (u.ToString().StartsWith("#"))
                        {
                            return Tools.ResolveUri(u, b).AbsoluteUri;
                        }
                        else if (Tools.IsValidBaseUri(b))
                        {
                            return Tools.ResolveUri(u, b).AbsoluteUri;
                        }
                        else
                        {
                            throw new RdfException("Cannot resolve a URI since the Base URI is not a valid for resolving Relative URIs against");
                        }
                    }
                }
            }
            else
            {
                if (uriref.Equals(String.Empty))
                {
                    throw new RdfException("Cannot use an Empty URI to refer to the document Base URI since there is no in-scope Base URI!");
                }

                try
                {
                    return new Uri(Tools.FixMalformedUriStrings(uriref), UriKind.Absolute).AbsoluteUri;
                }
                catch (UriFormatException)
                {
                    throw new RdfException("Cannot resolve a Relative URI Reference since there is no in-scope Base URI!");
                }
            }
        }

        /// <summary>
        /// Generic Helper Function which Resolves Uri References against a Base Uri
        /// </summary>
        /// <param name="uriref">Uri Reference to resolve</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns>Resolved Uri as a String</returns>
        /// <exception cref="UriFormatException">Uri Format Exception if one/both of the URIs is malformed</exception>
        public static Uri ResolveUri(Uri uriref, Uri baseUri)
        {
            Uri result = new Uri(baseUri, uriref);
            return result;
        }

        /// <summary>
        /// Resolves a QName into a Uri using the Namespace Mapper and Base Uri provided
        /// </summary>
        /// <param name="qname">QName to resolve</param>
        /// <param name="nsmap">Namespace Map to resolve against</param>
        /// <param name="baseUri">Base Uri to resolve against</param>
        /// <returns></returns>
        public static String ResolveQName(String qname, INamespaceMapper nsmap, Uri baseUri)
        {
            String output;

            if (qname.StartsWith(":"))
            {
                //QName in Default Namespace
                if (nsmap.HasNamespace(String.Empty))
                {
                    //Default Namespace Defined
                    output = nsmap.GetNamespaceUri(String.Empty).AbsoluteUri + qname.Substring(1);
                }
                else
                {
                    //No Default Namespace so use Base Uri
                    //These type of QNames are scoped to the local Uri regardless of the type of the Base Uri
                    //i.e. these always result in Hash URIs
                    if (baseUri != null)
                    {
                        output = baseUri.AbsoluteUri;
                        if (output.EndsWith("#"))
                        {
                            output += qname.Substring(1);
                        }
                        else
                        {
                            output += "#" + qname.Substring(1);
                        }
                    }
                    else
                    {
                        throw new RdfException("Cannot resolve the QName '" + qname + "' in the Default Namespace when there is no in-scope Base URI and no Default Namespace defined.  Did you forget to define a namespace for the : prefix?");
                    }
                }
            }
            else
            {
                //QName in some other Namespace
                String[] parts = qname.Split(new char[] { ':' }, 2);
                if (parts.Length == 1)
                {
                    output = nsmap.GetNamespaceUri(String.Empty).AbsoluteUri + parts[0];
                }
                else
                {
                    output = nsmap.GetNamespaceUri(parts[0]).AbsoluteUri + parts[1];
                }
            }

            return output;
        }

        /// <summary>
        /// Copies a Node so it can be used in another Graph since by default Triples cannot contain Nodes from more than one Graph
        /// </summary>
        /// <param name="original">Node to Copy</param>
        /// <param name="target">Graph to Copy into</param>
        /// <param name="keepOriginalGraphUri">Indicates whether the Copy should preserve the Graph Uri of the Node being copied</param>
        /// <returns></returns>
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(INode original, IGraph target, bool keepOriginalGraphUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Node so it can be used in another Graph since by default Triples cannot contain Nodes from more than one Graph
        /// </summary>
        /// <param name="original">Node to Copy</param>
        /// <param name="target">Graph to Copy into</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> Copying Blank Nodes may lead to unforseen circumstances since no remapping of IDs between Graphs is done
        /// </para>
        /// </remarks>
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(INode original, IGraph target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Node using another Node Factory
        /// </summary>
        /// <param name="original">Node to copy</param>
        /// <param name="target">Factory to copy into</param>
        /// <returns></returns>
        /// <remarks>
        /// <para>
        /// <strong>Warning:</strong> Copying Blank Nodes may lead to unforseen circumstances since no remapping of IDs between Factories is done
        /// </para>
        /// </remarks>
        [Obsolete("Copying Nodes is no longer required", true)]
        public static INode CopyNode(INode original, INodeFactory target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Triple from one Graph to another
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Graph to copy to</param>
        /// <returns></returns>
        [Obsolete("Copying Triples is no longer required", true)]
        public static Triple CopyTriple(Triple t, IGraph target)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Copies a Triple from one Graph to another
        /// </summary>
        /// <param name="t">Triple to copy</param>
        /// <param name="target">Graph to copy to</param>
        /// <param name="keepOriginalGraphUri">Indicates whether the Copy should preserve the Graph Uri of the Nodes being copied</param>
        /// <returns></returns>
        [Obsolete("Copying Triples is no longer required", true)]
        public static Triple CopyTriple(Triple t, IGraph target, bool keepOriginalGraphUri)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Does a quick and simple combination of the Hash Codes of two Objects
        /// </summary>
        /// <param name="x">First Object</param>
        /// <param name="y">Second Object</param>
        /// <returns></returns>
        public static int CombineHashCodes(Object x, Object y)
        {
            int hash = 17;
            hash = hash * 31 + x.GetHashCode();
            hash = hash * 31 + y.GetHashCode();
            return hash;
        }

#if DEBUG

        /// <summary>
        /// Prints Debugging Output to the Console Standard Out for a HTTP Web Request
        /// </summary>
        /// <param name="httpRequest">HTTP Web Request</param>
        /// <remarks><strong>Only available in Debug builds</strong></remarks>
        public static void HttpDebugRequest(HttpWebRequest httpRequest)
        {
            if (!Options.HttpDebugging) return;

            //Output the Request Headers
            Console.Error.WriteLine("# HTTP DEBUGGING #");
            Console.Error.WriteLine("HTTP Request to " + httpRequest.RequestUri.AbsoluteUri);
            Console.Error.WriteLine();
            Console.Error.WriteLine(httpRequest.Method);
            foreach (String header in httpRequest.Headers.AllKeys)
            {
                Console.Error.WriteLine(header + ":" + httpRequest.Headers[header]);
            }
            Console.Error.WriteLine();
        }

        /// <summary>
        /// Prints Debugging Output to the Console Standard Out for a HTTP Web Response
        /// </summary>
        /// <param name="httpResponse">HTTP Web Response</param>
        /// <remarks><strong>Only available in Debug builds</strong></remarks>
        public static void HttpDebugResponse(HttpWebResponse httpResponse)
        {
            if (!Options.HttpDebugging) return;

            //Output the Response Uri and Headers
            Console.Error.WriteLine();
            Console.Error.WriteLine("HTTP Response from " + httpResponse.ResponseUri.AbsoluteUri);
#if SILVERLIGHT
            Console.Error.WriteLine("HTTP " + (int)httpResponse.StatusCode + " " + httpResponse.StatusDescription);
#else
            Console.Error.WriteLine("HTTP/" + httpResponse.ProtocolVersion + " " + (int)httpResponse.StatusCode + " " + httpResponse.StatusDescription);
#endif
            Console.Error.WriteLine();
            foreach (String header in httpResponse.Headers.AllKeys)
            {
                Console.Error.WriteLine(header + ":" + httpResponse.Headers[header]);
            }
            Console.Error.WriteLine();

            if (Options.HttpFullDebugging)
            {
                //Output the actual Response
                Stream data = httpResponse.GetResponseStream();
                StreamReader reader = new StreamReader(data);
                while (!reader.EndOfStream)
                {
                    Console.Error.WriteLine(reader.ReadLine());
                }
                Console.Error.WriteLine();

                if (data.CanSeek) {
                    data.Seek(0, SeekOrigin.Begin);
                }
            }

            Console.Error.WriteLine("# END HTTP DEBUGGING #");
        }

#endif
    }
}
