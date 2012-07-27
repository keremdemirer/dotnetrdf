/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.ComponentModel;
#if SILVERLIGHT && !WINDOWS_PHONE
using System.ComponentModel.DataAnnotations;
#endif
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace VDS.RDF.Storage.Management.Provisioning.Stardog
{
    /// <summary>
    /// Abstract base implementation of a Store Template for creating Stardog Stores
    /// </summary>
    public abstract class BaseStardogTemplate
        : StoreTemplate
    {
        /// <summary>
        /// Creates a new Stardog Template
        /// </summary>
        /// <param name="id">Store ID</param>
        /// <param name="name">Template Name</param>
        /// <param name="descrip">Template Description</param>
        /// <param name="dbtype">Stardog Database Type</param>
        public BaseStardogTemplate(String id, String name, String descrip, String dbtype)
            : base(id, name, descrip)
        {
            //Index Options
            this.DatabaseType = dbtype;
            this.MinDifferentialIndexLimit = StardogConnector.DatabaseOptions.DefaultMinDifferentialIndexLimit;
            this.MaxDifferentialIndexLimit = StardogConnector.DatabaseOptions.DefaultMaxDifferentialIndexLimit;
            this.CanoncialiseLiterals = StardogConnector.DatabaseOptions.DefaultCanonicaliseLiterals;
            this.IndexNamedGraphs = StardogConnector.DatabaseOptions.DefaultNamedGraphIndexing;
            this.PersistIndexes = StardogConnector.DatabaseOptions.DefaultPersistIndex;
            this.PersistIndexesSynchronously = StardogConnector.DatabaseOptions.DefaultPersistIndexSync;
            this.AutoUpdateStatistics = StardogConnector.DatabaseOptions.DefaultAutoUpdateStats;

            //Integrity Constraint Validation
            this.IcvActiveGraphs = new List<string>();
            this.IcvEnabled = StardogConnector.DatabaseOptions.DefaultIcvEnabled;
            this.IcvReasoningMode = StardogConnector.DatabaseOptions.DefaultIcvReasoningMode;

            //Reasoning
            this.ConsistencyChecking = StardogConnector.DatabaseOptions.DefaultConsistencyChecking;
            this.EnablePunning = StardogConnector.DatabaseOptions.DefaultPunning;
            this.SchemaGraphs = new List<string>();

            //Search
            this.FullTextSearch = StardogConnector.DatabaseOptions.DefaultFullTextSearch;
            this.SearchReindexMode = StardogConnector.DatabaseOptions.SearchReIndexModeAsync;

            //Transactions
            this.DurableTransactions = StardogConnector.DatabaseOptions.DefaultDurableTransactions;
        }

        #region Index Options

        //index.differential.enable.limit
        //Sets the minimum size of the Stardog database before differential indexes are used.
        //index.differential.merge.limit
        //Sets the size in number of RDF triples before the differential indexes are merged to the main indexes.
        //index.literals.canonical
        //Enables RDF literal canonicalization. See literal canonicalization for details.
        //index.named.graphs
        //Enables optimized index support for named graphs; speeds SPARQL query evaluation with named graphs at the cost of some overhead for database loading and index maintenance.
        //index.persist
        //Enables persistent indexes.
        //index.persist.sync
        //Enables whether memory indexes are synchronously or asynchronously persisted to disk with respect to a transaction.
        //index.statistics.update.automatic
        //Sets whether statistics are maintained automatically.
        //index.type
        //Sets the index type (memory or disk).

        /// <summary>
        /// Gets the Database Type
        /// </summary>
        [Category("Index Options"), 
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Index Type"),
#else
         Display(Name="Index Type"),
#endif
#endif
         Description("The type of the index structures used for the database")]
        public String DatabaseType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/Sets the minimum differential index limit
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultMinDifferentialIndexLimit), 
         Category("Index Options"), 
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Differential Index Enabled Limit"), 
#else
         Display(Name="Differential Index Enabled Limit"),
#endif
#endif
         Description("The minimum size the Stardog database must be before differential indexes are used")]
        public int MinDifferentialIndexLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the maximum differential merge limit
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultMaxDifferentialIndexLimit), 
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Differential Index Merge Limit"), 
#else
         Display(Name="Differential Index Merge Limit"), 
#endif
#endif
         Description("The maximum size in triples of the differential index before it is merged into the main index")]
        public int MaxDifferentialIndexLimit
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether the database should canonicalise literals
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultCanonicaliseLiterals),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Canonicalise Literals"),
#else
         Display(Name="Canonicalise Literals"),
#endif
#endif
         Description("Sets whether literals are canonicalised before being indexed.  If enabled then literals will be transformed e.g. '1'^^xsd:byte => '1'^^xsd:integer, leave disabled to preserve data exactly as input")]
        public bool CanoncialiseLiterals
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to optimize indexes for named graph queries
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultNamedGraphIndexing),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Index Named Graphs"),
#else
         Display(Name="Index Named Graphs"),
#endif
#endif
         Description("Enables optimized index support for named graphs, improves query performance at the cost of load performance.  If your data is all in one graph or you infrequently query named graphs you may wish to disable this")]
        public bool IndexNamedGraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to persist indexes
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultPersistIndex),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Persistent Indexes"),
#else
         Display(Name="Persistent Indexes"),
#endif
#endif
         Description("Sets whether indexes are persistent")]
        public bool PersistIndexes
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultPersistIndexSync),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Persist Indexes Synchronously"),
#else
         Display(Name="Persist Indexes Synchronously"),
#endif
#endif
         Description("Sets whether indexes are persisted synchronously or asynchronously")]
        public bool PersistIndexesSynchronously
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets whether to automatically update statistics
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultAutoUpdateStats),
         Category("Index Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Auto Update Statistics"),
#else
         Display(Name="Auto Update Statistics"),
#endif
#endif
         Description("Sets whether statistics are automatically updated")]
        public bool AutoUpdateStatistics
        {
            get;
            set;
        }

        #endregion

        #region Integrity Constraint Validation Options

        //icv.active.graphs
        //Specifies which part of the database, in terms of named graphs, is checked with IC validation. to validate all the named graphs in the database.
        //icv.enabled
        //Determines whether ICV is active for the database; if true, all database mutations are subject to IC validation (i.e., "guard mode").
        //icv.reasoning-type
        //Determines what "reasoning level" is used during IC validation.

        /// <summary>
        /// Gets/Sets the active graphs for ICV
        /// </summary>
        [Category("Integrity Constraint Validation"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Active Graphs"), 
#else
         Display(Name="Active Graphs"), 
#endif
#endif
         Description("Sets the named graphs upon which integrity constraints are enforced")]
        public List<String> IcvActiveGraphs
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables ICV
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultIcvEnabled),
         Category("Integrity Constraint Validation"), 
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Enabled"), 
#else
         Display(Name="Enabled"), 
#endif
#endif
         Description("Enables integrity constraint validation for the database")]
        public bool IcvEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the reasoning mode for ICV
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultIcvReasoningMode),
         Category("Integrity Constraint Validation"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Reasoning Mode"), 
#else
         Display(Name="Reasoning Mode"),
#endif
#endif
         Description("Sets what reasoning mode is used during integrity constraint validation")]
        public StardogReasoningMode IcvReasoningMode
        {
            get;
            set;
        }


        #endregion

        #region Reasoning Options
        //reasoning.consistency.automatic
        //Enables automatic consistency checking with respect to a transaction.
        //reasoning.punning.enabled
        //Enables punning.
        //reasoning.schema.graphs
        //Determines which, if any, named graph or graphs contains the "tbox", i.e., the schema part of the data.

        /// <summary>
        /// Gets/Sets whether to perform automatic consistency checking on transactions
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultConsistencyChecking),
         Category("Reasoning Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Automatic Consistency Checking"),
#else
         Display(Name="Automatic Consistency Checking"),
#endif
#endif
         Description("Sets whether consistency checking is done with respect to transactions")]
        public bool ConsistencyChecking
        {
            get;
            set;
        }

        /// <summary>
        /// Enables/Disables punning
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultPunning),
         Category("Reasoning Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Enable Punning")
#else
         Display(Name="Enable Punning")
#endif
#endif
        ]
        public bool EnablePunning
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the graphs that contain the schema (TBox) that are used for reasoning
        /// </summary>
        [Category("Reasoning Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Schema Graphs"),
#else
         Display(Name="Schema Graphs"),
#endif
#endif
         Description("Sets the graphs considered to contain the schema (TBox) used for reasoning")]
        public List<String> SchemaGraphs
        {
            get;
            set;
        }


        #endregion

        #region Search Options

        //search.enabled
        //Enables semantic search on the database.
        //search.reindex.mode
        //Sets how search indexes are maintained.

        /// <summary>
        /// Enables/Disables Full Text search
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultFullTextSearch),
         Category("Search Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Enable Full Text Search"),
#else
         Display(Name="Enable Full Text Search"),
#endif
#endif
         Description("Enables full text search")]
        public bool FullTextSearch
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/Sets the Search re-indexing mode
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.SearchReIndexModeAsync),
         Category("Search Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Search Re-index Mode"),
#else
         Display(Name="Search Re-index Mode"),
#endif
#endif
         Description("Controls when search indexes are re-indexed, valid values are sync or async")]
        public String SearchReindexMode
        {
            get;
            set;
        }

        #endregion

        #region Transaction Options
        //transactions.durable
        //Enables durable transactions.

        /// <summary>
        /// Gets/Sets whether to use durable transactions
        /// </summary>
        [DefaultValue(StardogConnector.DatabaseOptions.DefaultDurableTransactions),
         Category("Transaction Options"),
#if !WINDOWS_PHONE
#if !SILVERLIGHT
         DisplayName("Durable Transactions"),
#else
         Display(Name="Durable Transactions"),
#endif
#endif
         Description("Enables durable transactions")]
        public bool DurableTransactions
        {
            get;
            set;
        }

        #endregion

        /// <summary>
        /// Validates that the template is filled out such that a store can be created from it
        /// </summary>
        /// <returns>Enumeration of errors that occurred</returns>
        public override IEnumerable<string> Validate()
        {
            List<String> errors = new List<string>();
            if (!StardogConnector.DatabaseOptions.IsValidDatabaseName(this.ID))
            {
                errors.Add("Database Name " + this.ID + " is invalid, Stardog database names must match the regular expression " + StardogConnector.DatabaseOptions.ValidDatabaseNamePattern);
            }
            if (!StardogConnector.DatabaseOptions.IsValidDatabaseType(this.DatabaseType))
            {
                errors.Add("Database Type " + this.DatabaseType + " is invalid");
            }
            if (!StardogConnector.DatabaseOptions.IsValidSearchReIndexMode(this.SearchReindexMode))
            {
                errors.Add("Search Re-index Mode " + this.SearchReindexMode + " is invalid, only sync or async are currently permitted");
            }
            foreach (String uri in this.SchemaGraphs)
            {
                if (!StardogConnector.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("Schema Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            foreach (String uri in this.IcvActiveGraphs)
            {
                if (!StardogConnector.DatabaseOptions.IsValidNamedGraph(uri))
                {
                    errors.Add("ICV Active Graphs contains invalid Graph URI '" + uri + "' - must use a valid URI, default or *");
                }
            }
            this.ValidateInternal(errors);
            return errors;
        }

        /// <summary>
        /// Does any additional validation a derived template may require
        /// </summary>
        /// <param name="errors">Error collection to add to</param>
        protected virtual void ValidateInternal(List<String> errors) { }

        /// <summary>
        /// Gets the JSON Template for creating a store
        /// </summary>
        /// <returns></returns>
        public JObject GetTemplateJson()
        {
            //Set up the basic template
            JObject template = new JObject();
            template.Add("dbname", new JValue(this.ID));

            //Build up the options object
            //Don't bother included non-required options if the user hasn't adjusted them from their defaults
            JObject options = new JObject();

            //Index Options
            options.Add(StardogConnector.DatabaseOptions.IndexType, new JValue(this.DatabaseType.ToLower()));
            if (this.MinDifferentialIndexLimit != StardogConnector.DatabaseOptions.DefaultMinDifferentialIndexLimit) options.Add(StardogConnector.DatabaseOptions.IndexDifferentialEnableLimit, new JValue(this.MinDifferentialIndexLimit));
            if (this.MaxDifferentialIndexLimit != StardogConnector.DatabaseOptions.DefaultMaxDifferentialIndexLimit) options.Add(StardogConnector.DatabaseOptions.IndexDifferentialMergeLimit, new JValue(this.MaxDifferentialIndexLimit));
            if (this.CanoncialiseLiterals != StardogConnector.DatabaseOptions.DefaultCanonicaliseLiterals) options.Add(StardogConnector.DatabaseOptions.IndexLiteralsCanonical, new JValue(this.CanoncialiseLiterals));
            if (this.IndexNamedGraphs != StardogConnector.DatabaseOptions.DefaultNamedGraphIndexing) options.Add(StardogConnector.DatabaseOptions.IndexNamedGraphs, new JValue(this.IndexNamedGraphs));
            if (this.PersistIndexes != StardogConnector.DatabaseOptions.DefaultPersistIndex) options.Add(StardogConnector.DatabaseOptions.IndexPersistTrue, new JValue(this.PersistIndexes));
            if (this.PersistIndexesSynchronously != StardogConnector.DatabaseOptions.DefaultPersistIndexSync) options.Add(StardogConnector.DatabaseOptions.IndexPersistSync, new JValue(this.PersistIndexesSynchronously));
            if (this.AutoUpdateStatistics != StardogConnector.DatabaseOptions.DefaultAutoUpdateStats) options.Add(StardogConnector.DatabaseOptions.IndexStatisticsAutoUpdate, new JValue(this.AutoUpdateStatistics));

            //ICV Options
            if (this.IcvActiveGraphs.Count > 0) options.Add(StardogConnector.DatabaseOptions.IcvActiveGraphs, new JValue(String.Join(",", this.IcvActiveGraphs.ToArray())));
            if (this.IcvEnabled != StardogConnector.DatabaseOptions.DefaultIcvEnabled) options.Add(StardogConnector.DatabaseOptions.IcvEnabled, new JValue(this.IcvEnabled));
            if (this.IcvReasoningMode != StardogConnector.DatabaseOptions.DefaultIcvReasoningMode) options.Add(StardogConnector.DatabaseOptions.IcvReasoningType, new JValue(this.IcvReasoningMode.ToString()));
            
            //Reasoning
            if (this.ConsistencyChecking != StardogConnector.DatabaseOptions.DefaultConsistencyChecking) options.Add(StardogConnector.DatabaseOptions.ReasoningAutoConsistency, new JValue(this.ConsistencyChecking));
            if (this.EnablePunning != StardogConnector.DatabaseOptions.DefaultPunning) options.Add(StardogConnector.DatabaseOptions.ReasoningPunning, new JValue(this.EnablePunning));
            if (this.SchemaGraphs.Count > 0) options.Add(StardogConnector.DatabaseOptions.ReasoningSchemaGraphs, new JValue(String.Join(",", this.SchemaGraphs.ToArray())));

            //Search
            if (this.FullTextSearch != StardogConnector.DatabaseOptions.DefaultFullTextSearch) options.Add(StardogConnector.DatabaseOptions.SearchEnabled, new JValue(this.FullTextSearch));
            if (this.SearchReindexMode.ToLower() != StardogConnector.DatabaseOptions.SearchReIndexModeAsync) options.Add(StardogConnector.DatabaseOptions.SearchReIndexMode, new JValue(this.SearchReindexMode.ToLower()));

            //Transactions
            if (this.DurableTransactions != StardogConnector.DatabaseOptions.DefaultDurableTransactions) options.Add(StardogConnector.DatabaseOptions.TransactionsDurable, new JValue(this.DurableTransactions));

            //Add options to the Template
            template.Add("options", options);

            //Add empty files list
            template.Add("files", new JArray());

            return template;
        }
    }
}