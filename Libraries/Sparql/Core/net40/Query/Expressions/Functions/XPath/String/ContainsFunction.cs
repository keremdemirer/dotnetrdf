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

using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions.Factories;

namespace VDS.RDF.Query.Expressions.Functions.XPath.String
{
    /// <summary>
    /// Represents the XPath fn:contains() function
    /// </summary>
    public class ContainsFunction
        : BaseBinaryStringFunction
    {
        /// <summary>
        /// Creates a new XPath Contains function
        /// </summary>
        /// <param name="stringExpr">Expression</param>
        /// <param name="searchExpr">Search Expression</param>
        public ContainsFunction(IExpression stringExpr, IExpression searchExpr)
            : base(stringExpr, searchExpr) { }

        /// <summary>
        /// Gets the Value of the function as applied to the given String Literal and Argument
        /// </summary>
        /// <param name="stringLit">Simple/String typed Literal</param>
        /// <param name="arg">Argument</param>
        /// <returns></returns>
        protected override IValuedNode EvaluateInternal(IValuedNode stringLit, IValuedNode arg)
        {
            if (stringLit.Value.Equals(string.Empty))
            {
                //Empty string cannot contain anything
                return new BooleanNode(false);
            }
            if (arg.Value.Equals(string.Empty))
            {
                //Any non-empty string contains the empty string
                return new BooleanNode(true);
            }
            //Evalute the Contains
            return new BooleanNode(stringLit.Value.Contains(arg.Value));
        }

        public override IExpression Copy(IExpression arg1, IExpression arg2)
        {
            return new ContainsFunction(arg1, arg2);
        }

        /// <summary>
        /// Gets the Functor of the Expression
        /// </summary>
        public override string Functor
        {
            get
            {
                return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.Contains;
            }
        }
    }
}