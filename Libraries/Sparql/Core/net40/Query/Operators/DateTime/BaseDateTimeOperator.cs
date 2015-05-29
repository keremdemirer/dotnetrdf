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
using VDS.RDF.Configuration;
using VDS.RDF.Nodes;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Specifications;

namespace VDS.RDF.Query.Operators.DateTime
{
    /// <summary>
    /// Abstract base operator for date time operations
    /// </summary>
    public abstract class BaseDateTimeOperator
        : BaseOperator
    {
        /// <summary>
        /// Gets whether the arguments are applicable for this operator
        /// </summary>
        /// <param name="ns">Arguments</param>
        /// <returns></returns>
        public override bool IsApplicable(params IValuedNode[] ns)
        {
            return !SparqlOptions.StrictOperators 
                   && ns != null
                   && ns.Length == 2
                   && ns.All(n => n!= null) 
                   && ns[0].EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDateTime) 
                   && (ns[1].EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDayTimeDuration) || ns[1].EffectiveType.Equals(XmlSpecsHelper.XmlSchemaDataTypeDuration));
        }
    }
}