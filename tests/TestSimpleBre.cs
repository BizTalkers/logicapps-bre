using Xunit;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System;
using System.Xml.Linq;
using FluentAssertions;

namespace ProcessManager.Tests
{
    public class TestSimpleBre
    {
        public TestSimpleBre()
        {
            // Set the environment variable for the test
            Environment.SetEnvironmentVariable("AzureWebJobsStorage", "UseDevelopmentStorage=true");
        }

        [Fact]
        public async Task RunRules_Should_Return_RuleExecutionResult()
        {
            // Arrange
            var assembly = Assembly.GetExecutingAssembly();

            // Load request
            var requestResourceName = "ProcessManager.UnitTests.InputFiles.SimpleSample.xml";
            string inputXml;
            using (Stream stream = assembly.GetManifestResourceStream(requestResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                inputXml = reader.ReadToEnd();
            }

            // Load GatekeeperRuleSet.xml
            var ruleSetResourceName = "ProcessManager.UnitTests.InputFiles.SampleRuleset.xml";
            string ruleSetXml;
            using (Stream stream = assembly.GetManifestResourceStream(ruleSetResourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                ruleSetXml = reader.ReadToEnd();
            }

            // Get the base directory of the executing assembly
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // Create the Artifacts/Rules directory
            var rulesFolder = Path.Combine(baseDirectory, "Artifacts", "Rules");
            Directory.CreateDirectory(rulesFolder);

            // Write SampleRuleSet.xml to the Artifacts/Rules directory
            var ruleSetPath = Path.Combine(rulesFolder, "SampleRuleSet.xml");
            File.WriteAllText(ruleSetPath, ruleSetXml);

            var ruleSetName = "SampleRuleSet";
            var documentType = "SchemaUser";
            var loggerStub = new LoggerStub<BreRunner>();
            var loggerFactoryStub = new LoggerFactoryStub(loggerStub);
            var breRunner = new BreRunner(loggerFactoryStub);

            // Act
            var result = await breRunner.RunRules(ruleSetName, documentType, inputXml, 1100, "98052");

            // Assert - the result should be as follows
            // <ns0:Root xmlns:ns0="http://BizTalk_Server_Project1.SchemaUser">
            //   <UserDetails>
            //     <Age>70</Age>
            //     <Name>UserName</Name>
            //     <zipCode>98053</zipCode>
            //   </UserDetails>
            //   <Status>
            //     <Gold>true</Gold>
            //     <Discount>5</Discount>
            //   </Status>
            // </ns0:Root>

            // Parse the XML response
            var xmlDoc = XDocument.Parse(result.XmlDoc);

            //obtain value of Discount element
            var discount = xmlDoc.Root.Element("Status").Element("Discount");

            discount.Value.Should().Be("5");

        }
    }

}
