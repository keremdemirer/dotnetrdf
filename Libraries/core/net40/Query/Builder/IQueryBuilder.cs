using System;
using System.Collections.Generic;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder
{
    public interface IQueryBuilder 
    {
        /// <summary>
        /// Applies the DISTINCT modifier if the Query is a SELECT, otherwise leaves query unchanged (since results from any other query are DISTINCT by default)
        /// </summary>
        /// <param name="q">Query</param>
        /// <returns></returns>
        IQueryBuilder Distinct();

        IQueryBuilder Where(params ITriplePattern[] triplePatterns);
        IQueryBuilder Where(Action<ITriplePatternBuilder> buildTriplePatterns);

        IQueryBuilder Optional(Action<ITriplePatternBuilder> buildTriplePatterns);
        IQueryBuilder Optional(params ITriplePattern[] triplePatterns);

        IQueryBuilder Filter(ISparqlExpression expr);
        IQueryBuilder Limit(int limit);
        IQueryBuilder Offset(int offset);
        IQueryBuilder Slice(int limit, int offset);
        SparqlQuery GetExecutableQuery();

        INamespaceMapper NamespaceMapper { get; }
    }
}