/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Construct;

namespace VDS.RDF.Query.Patterns
{

    /// <summary>
    /// Class for representing Triple Patterns in SPARQL Queries
    /// </summary>
    public class TriplePattern
        : BaseTriplePattern, IMatchTriplePattern, IConstructTriplePattern, IComparable<TriplePattern>
    {
        private readonly TripleIndexType _indexType = TripleIndexType.None;
        private readonly PatternItem _subj, _pred, _obj;

        /// <summary>
        /// Creates a new Triple Pattern
        /// </summary>
        /// <param name="subj">Subject Pattern</param>
        /// <param name="pred">Predicate Pattern</param>
        /// <param name="obj">Object Pattern</param>
        public TriplePattern(PatternItem subj, PatternItem pred, PatternItem obj)
        {
            this._subj = subj;
            if (pred is BlankNodePattern)
            {
                throw new RdfParseException("Cannot use a Triple Pattern with a Blank Node Predicate in a SPARQL Query");
            }
            this._pred = pred;
            this._obj = obj;

            // Decide on the Index Type
            if (this._subj is NodeMatchPattern)
            {
                if (Options.FullTripleIndexing)
                {
                    if (this._pred is NodeMatchPattern)
                    {
                        this._indexType = TripleIndexType.SubjectPredicate;
                    }
                    else if (this._obj is NodeMatchPattern)
                    {
                        this._indexType = TripleIndexType.SubjectObject;
                    }
                    else
                    {
                        this._indexType = TripleIndexType.Subject;
                    }
                }
                else
                {
                    this._indexType = TripleIndexType.Subject;
                }
            }
            else if (this._pred is NodeMatchPattern)
            {
                if (Options.FullTripleIndexing)
                {
                    if (this._obj is NodeMatchPattern)
                    {
                        this._indexType = TripleIndexType.PredicateObject;
                    }
                    else
                    {
                        this._indexType = TripleIndexType.Predicate;
                    }
                }
                else
                {
                    this._indexType = TripleIndexType.Predicate;
                }                
            }
            else if (this._obj is NodeMatchPattern)
            {
                this._indexType = TripleIndexType.Object;
            }

            // Determine variables used
            if (this._subj.VariableName != null) this._vars.Add(this._subj.VariableName);
            if (this._pred.VariableName != null)
            {
                if (!this._vars.Contains(this._pred.VariableName))
                {
                    this._vars.Add(this._pred.VariableName);
                }
                else
                {
                    this._pred.Repeated = true;
                }
            }
            if (this._obj.VariableName != null)
            {
                if (!this._vars.Contains(this._obj.VariableName))
                {
                    this._vars.Add(this._obj.VariableName);
                }
                else
                {
                    this.Object.Repeated = true;
                }
            }
            this._vars.Sort();
            if (this._vars.Count == 0) this._indexType = TripleIndexType.NoVariables;
        }

        /// <summary>
        /// Gets whether a given Triple is accepted by this Pattern in the given Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="obj">Triple to test</param>
        /// <returns></returns>
        public bool Accepts(SparqlEvaluationContext context, Triple obj)
        {
            if (!this._pred.Repeated && !this._obj.Repeated)
            {
                return (this._subj.Accepts(context, obj.Subject) && this._pred.Accepts(context, obj.Predicate) && this._obj.Accepts(context, obj.Object));
            }
            else if (this._pred.Repeated && !this._obj.Repeated)
            {
                return (this._subj.Accepts(context, obj.Subject) && obj.Subject.Equals(obj.Predicate) && this._obj.Accepts(context, obj.Object));
            }
            else if (!this._pred.Repeated && this._obj.Repeated)
            {
                return (this._subj.Accepts(context, obj.Subject) && this._pred.Accepts(context, obj.Predicate) && obj.Subject.Equals(obj.Object));
            }
            else
            {
                return (this._subj.Accepts(context, obj.Subject) && obj.Subject.Equals(obj.Predicate) && obj.Subject.Equals(obj.Object));
            }
        }

        /// <summary>
        /// Gets the pattern type
        /// </summary>
        public override TriplePatternType PatternType
        {
            get 
            {
                return TriplePatternType.Match;
            }
        }

        /// <summary>
        /// Gets the Index Type we will use for this Pattern
        /// </summary>
        public TripleIndexType IndexType
        {
            get
            {
                return this._indexType;
            }
        }

        /// <summary>
        /// Subject Pattern
        /// </summary>
        public PatternItem Subject
        {
            get
            {
                return this._subj;
            }
        }

        /// <summary>
        /// Predicate Pattern
        /// </summary>
        public PatternItem Predicate
        {
            get
            {
                return this._pred;
            }
        }

        /// <summary>
        /// Object Pattern
        /// </summary>
        public PatternItem Object
        {
            get
            {
                return this._obj;
            }
        }

        /// <summary>
        /// Returns all variables mentioned as a match guarantees all variables are bound
        /// </summary>
        public override IEnumerable<string> FixedVariables
        {
            get { return this.Variables; }
        }

        /// <summary>
        /// Returns an empty enumeration as a match guarantees all variables are bound
        /// </summary>
        public override IEnumerable<string> FloatingVariables { get { return Enumerable.Empty<String>(); } }

        /// <summary>
        /// Returns whether the Triple Pattern is an accept all
        /// </summary>
        /// <remarks>
        /// True if all three Pattern Items are <see cref="VariablePattern">VariablePattern</see> and all the Variables names are distinct
        /// </remarks>
        public override bool IsAcceptAll
        {
            get
            {
                return (this._subj is VariablePattern && this._pred is VariablePattern && this._obj is VariablePattern)  &&
                    (this._subj.VariableName != this._pred.VariableName && this._pred.VariableName != this._obj.VariableName && this._subj.VariableName != this._obj.VariableName);
            }

        }

        /// <summary>
        /// Evaluates a Triple Pattern in the given Evaluation Context
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        public override void Evaluate(SparqlEvaluationContext context)
        {
            if (this._indexType == TripleIndexType.NoVariables)
            {
                // If there are no variables then at least one Triple must match or we abort
                INode s = ((NodeMatchPattern)this._subj).Node;
                INode p = ((NodeMatchPattern)this._pred).Node;
                INode o = ((NodeMatchPattern)this._obj).Node;
                if (context.Data.ContainsTriple(new Triple(s, p, o)))
                {
                    context.OutputMultiset = new IdentityMultiset();
                }
                else
                {
                    context.OutputMultiset = new NullMultiset();
                }
            }
            else
            {
                this.FindResults(context, this.GetTriples(context));
            }
        }

        /// <summary>
        /// Gets the Enumeration of Triples that should be assessed for matching the pattern
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <returns></returns>
        public IEnumerable<Triple> GetTriples(SparqlEvaluationContext context)
        {
            INode subj, pred, obj;

            // Stuff for more precise indexing
            IEnumerable<INode> values = null;
            IEnumerable<ISet> valuePairs = null;
            String subjVar = this._subj.VariableName;
            String predVar = this._pred.VariableName;
            String objVar = this._obj.VariableName;
            bool boundSubj = (subjVar != null && context.InputMultiset.ContainsVariable(subjVar));
            bool boundPred = (predVar != null && context.InputMultiset.ContainsVariable(predVar));
            bool boundObj = (objVar != null && context.InputMultiset.ContainsVariable(objVar));

            switch (this._indexType)
            {
                case TripleIndexType.Subject:
                    subj = ((NodeMatchPattern)this._subj).Node;
                    if (boundPred)
                    {
                        if (boundObj)
                        {
                            valuePairs = (from set in context.InputMultiset.Sets
                                          where set.ContainsVariable(predVar) && set.ContainsVariable(objVar)
                                          select set).Distinct(new SetDistinctnessComparer(new String[] { predVar, objVar }));
                            return (from set in valuePairs
                                    where set[predVar] != null && set[objVar] != null
                                    select this.CreateTriple(subj, set[predVar], set[objVar])).Where(t => context.Data.ContainsTriple(t));
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(predVar)
                                      select set[predVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubjectPredicate(subj, value)
                                    select t);
                        }
                    }
                    else if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithSubjectObject(subj, value)
                                select t);
                    }
                    else
                    {
                        return context.Data.GetTriplesWithSubject(subj);
                    }

                case TripleIndexType.SubjectPredicate:
                    subj = ((NodeMatchPattern)this._subj).Node;
                    pred = ((NodeMatchPattern)this._pred).Node;

                    if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                select this.CreateTriple(subj, pred, value)).Where(t => context.Data.ContainsTriple(t));
                    }
                    else
                    {
                        return context.Data.GetTriplesWithSubjectPredicate(subj, pred);
                    }

                case TripleIndexType.SubjectObject:
                    subj = ((NodeMatchPattern)this._subj).Node;
                    obj = ((NodeMatchPattern)this._obj).Node;

                    if (boundPred)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(predVar)
                                  select set[predVar]).Distinct();
                        return (from value in values
                                where value != null
                                select this.CreateTriple(subj, value, obj)).Where(t => context.Data.ContainsTriple(t));
                    }
                    else
                    {
                        return context.Data.GetTriplesWithSubjectObject(subj, obj);
                    }

                case TripleIndexType.Predicate:
                    pred = ((NodeMatchPattern)this._pred).Node;
                    if (boundSubj)
                    {
                        if (boundObj)
                        {
                            valuePairs = (from set in context.InputMultiset.Sets
                                          where set.ContainsVariable(subjVar) && set.ContainsVariable(objVar)
                                          select set).Distinct(new SetDistinctnessComparer(new String[] { subjVar, objVar }));
                            return (from set in valuePairs
                                    where set[subjVar] != null && set[objVar] != null
                                    select this.CreateTriple(set[subjVar], pred, set[objVar])).Where(t => context.Data.ContainsTriple(t));
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(subjVar)
                                      select set[subjVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubjectPredicate(value, pred)
                                    select t);
                        }
                    }
                    else if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithPredicateObject(pred, value)
                                select t);
                    }
                    else
                    {
                        return context.Data.GetTriplesWithPredicate(pred);
                    }

                case TripleIndexType.PredicateObject:
                    pred = ((NodeMatchPattern)this._pred).Node;
                    obj = ((NodeMatchPattern)this._obj).Node;

                    if (boundSubj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(subjVar)
                                  select set[subjVar]).Distinct();
                        return (from value in values
                                where value != null
                                select this.CreateTriple(value, pred, obj)).Where(t => context.Data.ContainsTriple(t));
                    }
                    else
                    {
                        return context.Data.GetTriplesWithPredicateObject(pred, obj);
                    }

                case TripleIndexType.Object:
                    obj = ((NodeMatchPattern)this._obj).Node;
                    if (boundSubj)
                    {
                        if (boundPred)
                        {
                            valuePairs = (from set in context.InputMultiset.Sets
                                          where set.ContainsVariable(subjVar) && set.ContainsVariable(predVar)
                                          select set).Distinct(new SetDistinctnessComparer(new String[] { subjVar, predVar }));
                            return (from set in valuePairs
                                    where set[subjVar] != null && set[predVar] != null
                                    select this.CreateTriple(set[subjVar], set[predVar], obj)).Where(t => context.Data.ContainsTriple(t));
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(subjVar)
                                      select set[subjVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubjectObject(value, obj)
                                    select t);
                        }
                    }
                    else if (boundPred)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(predVar)
                                  select set[predVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithPredicateObject(value, obj)
                                select t);
                    }
                    else
                    {
                        return context.Data.GetTriplesWithObject(obj);
                    }

                case TripleIndexType.NoVariables:
                    // If there are no variables then at least one Triple must match or we abort
                    INode s, p, o;
                    s = ((NodeMatchPattern)this._subj).Node;
                    p = ((NodeMatchPattern)this._pred).Node;
                    o = ((NodeMatchPattern)this._obj).Node;
                    if (context.Data.ContainsTriple(new Triple(s, p, o)))
                    {
                        return new Triple(s, p, o).AsEnumerable();
                    }
                    else
                    {
                        return Enumerable.Empty<Triple>();
                    }

                case TripleIndexType.None:
                    // This means we got a pattern like ?s ?p ?o so we want to use whatever bound variables 
                    // we have to reduce the triples we return as far as possible
                    // TODO: Add handling for all the cases here
                    if (boundSubj)
                    {
                        if (boundPred)
                        {
                            return context.Data.Triples;
                        }
                        else if (boundObj)
                        {
                            return context.Data.Triples;
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(subjVar)
                                      select set[subjVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithSubject(value)
                                    select t);
                        }
                    }
                    else if (boundPred)
                    {
                        if (boundObj)
                        {
                            return context.Data.Triples;
                        }
                        else
                        {
                            values = (from set in context.InputMultiset.Sets
                                      where set.ContainsVariable(predVar)
                                      select set[predVar]).Distinct();
                            return (from value in values
                                    where value != null
                                    from t in context.Data.GetTriplesWithPredicate(value)
                                    select t);
                        }
                    }
                    else if (boundObj)
                    {
                        values = (from set in context.InputMultiset.Sets
                                  where set.ContainsVariable(objVar)
                                  select set[objVar]).Distinct();
                        return (from value in values
                                where value != null
                                from t in context.Data.GetTriplesWithObject(value)
                                select t);
                    }
                    else
                    {
                        return context.Data.Triples;
                    }

                default:
                    return context.Data.Triples;
            }
        }

        private Triple CreateTriple(INode s, INode p, INode o)
        {
            IGraph target = s.Graph;
            if (target == null)
            {
                target = p.Graph;
                if (target == null) target = o.Graph;
            }
            return new Triple(s.CopyNode(target), p.CopyNode(target), o.CopyNode(target));
        }

        /// <summary>
        /// Takes an enumerable and extracts Triples which match this pattern as results
        /// </summary>
        /// <param name="context">SPARQL Evaluation Context</param>
        /// <param name="ts">Enumerable of Triples</param>
        private void FindResults(SparqlEvaluationContext context, IEnumerable<Triple> ts)
        {
            foreach (Triple t in ts)
            {
                if (this.Accepts(context, t))
                {
                    context.OutputMultiset.Add(this.CreateResult(t));
                }
            }
        }

        /// <summary>
        /// Generates a Result Set for a Triple that matches the Pattern
        /// </summary>
        /// <param name="t">Triple</param>
        /// <returns></returns>
        public ISet CreateResult(Triple t)
        {
            Set s = new Set();
            if (this._subj.VariableName != null)
            {
                s.Add(this._subj.VariableName, t.Subject);
            }
            if (this._pred.VariableName != null && !this._pred.Repeated)
            {
                s.Add(this._pred.VariableName, t.Predicate);
            }
            if (this._obj.VariableName != null && !this._obj.Repeated)
            {
                s.Add(this._obj.VariableName, t.Object);
            }
            return s;
        }

        /// <summary>
        /// Constructs a Triple from a Set based on this Triple Pattern
        /// </summary>
        /// <param name="context">Construct Context</param>
        /// <returns></returns>
        public Triple Construct(ConstructContext context)
        {
            return new Triple(Tools.CopyNode(this._subj.Construct(context), context.Graph), Tools.CopyNode(this._pred.Construct(context), context.Graph), Tools.CopyNode(this._obj.Construct(context), context.Graph));
        }

        /// <summary>
        /// Gets whether the Pattern contains no Variables of any kind
        /// </summary>
        public bool HasNoVariables
        {
            get
            {
                return (this._indexType == TripleIndexType.NoVariables);
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored)
        /// </summary>
        public bool HasNoExplicitVariables
        {
            get
            {
                return (this._subj is NodeMatchPattern || this._subj is BlankNodePattern || this._subj is FixedBlankNodePattern) &&
                       (this._pred is NodeMatchPattern || this._pred is BlankNodePattern || this._pred is FixedBlankNodePattern) &&
                       (this._obj is NodeMatchPattern || this._obj is BlankNodePattern || this._obj is FixedBlankNodePattern);
            }
        }

        /// <summary>
        /// Gets whether the Pattern contains no Explicit Variables (i.e. Blank Node Variables are ignored)
        /// </summary>
        public override bool HasNoBlankVariables
        {
            get
            {
                return (this._subj is NodeMatchPattern || this._subj is VariablePattern || this._subj is FixedBlankNodePattern) &&
                       (this._pred is NodeMatchPattern || this._pred is VariablePattern || this._pred is FixedBlankNodePattern) &&
                       (this._obj is NodeMatchPattern || this._obj is VariablePattern || this._obj is FixedBlankNodePattern);
            }
        }

        /// <summary>
        /// Compares a triple pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(TriplePattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Compares a triple pattern to another
        /// </summary>
        /// <param name="other">Pattern</param>
        /// <returns></returns>
        public int CompareTo(IMatchTriplePattern other)
        {
            return base.CompareTo(other);
        }

        /// <summary>
        /// Gets the String representation of this Pattern
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this._subj.ToString() + " " + this._pred.ToString() + " " + this._obj.ToString();
        }
    }
}