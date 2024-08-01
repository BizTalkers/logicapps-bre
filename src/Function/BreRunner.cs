//------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
//------------------------------------------------------------

namespace ProcessManager
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.Azure.Functions.Extensions.Workflows;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.Workflows.RuleEngine;
    using Microsoft.Extensions.Logging;
    using System.Xml;
    using System.IO;
    using Azure.Storage.Blobs;

    /// <summary>
    /// Represents the BreFunctionApp flow invoked function.
    /// </summary>
    public class BreRunner
    {
        private readonly ILogger<BreRunner> _logger;

        public BreRunner(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<BreRunner>();
        }

        /// <summary>
        /// Executes the logic app workflow.
        /// </summary>
        /// <param name="ruleSetName">The rule set name.</param>
        /// <param name="documentType">document type of input xml.</param>
        /// <param name="inputXml">input xml type fact</param>
        /// <param name="purchaseAmount">purchase amount, value used to create .NET fact </param>
        /// <param name="zipCode">zip code value used to create .NET fact .</param>
        [FunctionName("RunRules")]
        public Task<RuleExecutionResult> RunRules(
            [WorkflowActionTrigger] string ruleSetName,
            string documentType,
            string inputXml,
            int purchaseAmount, 
            string zipCode)
        {
            /***** Summary of steps below *****
             * 1. Get the rule set to Execute 
             * 2. Check if the rule set was retrieved successfully
             * 3. create the rule engine object
             * 4. Create TypedXmlDocument facts for all xml document facts
             * 5. Initialize .NET facts
             * 6. Execute rule engine
             * 7. Retrieve relevant updates facts and send them back
             */
            try
            {
                // Get the ruleset based on ruleset name
                var ruleExplorer = new FileStoreRuleExplorer();
                var ruleSet = ruleExplorer.GetRuleSet(ruleSetName);

                // Check if ruleset exists
                if (ruleSet == null)
                {
                    // Log an error in finding the rule set
                    this._logger.LogCritical($"RuleSet instance for '{ruleSetName}' was not found(null)");
                    throw new Exception($"RuleSet instance for '{ruleSetName}' was not found.");
                }

                // Create rule engine instance
                var ruleEngine = new RuleEngine(ruleSet: ruleSet);
                string trackingPath = Path.Combine(Path.GetTempPath(), $"{ruleSetName}-bre-tracking.txt");
                DebugTrackingInterceptor debugTrackingInterceptor = BreHelper.GetTrackingInterceptor(trackingPath);
                ruleEngine.TrackingInterceptor = debugTrackingInterceptor;

                // Create a typedXml Fact(s) from input xml(s)
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(inputXml);
                var typedXmlDocument = new TypedXmlDocument(documentType, doc);

                // Initialize .NET facts
                var currentPurchase = new ContosoPurchase(purchaseAmount, zipCode);

                //run rules engine
                ruleEngine.Execute(new object[] { typedXmlDocument });
                BreHelper.CopyTrackingFileToBlob(trackingPath);

                // Send the relevant results(facts) back
                var updatedDoc = typedXmlDocument.Document as XmlDocument;
                var ruleExectionOutput = new RuleExecutionResult()
                {
                    XmlDoc = updatedDoc.OuterXml
                };

                return Task.FromResult(ruleExectionOutput);
            }
            catch (RuleEngineException ruleEngineException)
            {
                // Log any rule engine exceptions
                this._logger.LogCritical(ruleEngineException.ToString());
                throw;
            }
        }


        /// <summary>
        /// Results of the rule execution
        /// </summary>
        public class RuleExecutionResult
        {
            /// <summary>
            /// rules updated xml document
            /// </summary>
            public string XmlDoc { get; set; }
        }
    }
}
