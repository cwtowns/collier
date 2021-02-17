using FluentAssertions;
using Collier.Monitoring.Gpu;
using Newtonsoft.Json.Linq;
using System.Text;
using Xunit;

namespace CollierTests.Monitoring
{
    public class Monitoring
    {
        [Fact]
        public void CommentsAreIgnored()
        {
            var data = "==============NVSMI LOG==============";
            var parser = new NvidiaSmiParser();
            var jsonObject = parser.Parse(data);

            jsonObject.Count.Should().Be(0, "we passed only a comment");
        }

        [Fact]
        public void EmptyDataIsIgnored()
        {
            var data = new StringBuilder();
            data.AppendLine("    ");
            data.AppendLine("        ");
            data.AppendLine("    ");
            data.AppendLine("    ");
            var parser = new NvidiaSmiParser();
            var jsonObject = parser.Parse(data.ToString());

            jsonObject.Count.Should().Be(0, "we passed only empty multilined strings");
        }

        [Fact]
        public void SimplePropertyLookup()
        {
            var data = new StringBuilder();
            data.AppendLine("foo     :    bar");
            var parser = new NvidiaSmiParser();
            var jsonObject = parser.Parse(data.ToString());

            jsonObject.Count.Should().Be(1, "we passed a single property as our datasource");
            jsonObject.Value<string>("foo").Should().Be("bar", "property lookup should return correct value.");
        }

        [Fact]
        public void ObjectValueLookup()
        {
            var data = new StringBuilder();
            data.AppendLine("someObject");
            data.AppendLine("    foo    : bar    ");
            var parser = new NvidiaSmiParser();
            var jsonObject = parser.Parse(data.ToString());

            jsonObject["someObject"].Should().NotBeNull("properties with object values should exist");

            jsonObject = jsonObject["someObject"] as JObject;
            jsonObject.Should().NotBeNull("properties with object values should be JObject types");

            jsonObject.Value<string>("foo").Should().Be("bar", "property lookup should return correct value");
        }

        [Fact]
        public void MovingBackUpPropertyHierarchy()
        {
            var data = new StringBuilder();
            data.AppendLine("someObject");
            data.AppendLine("    foo    : bar    ");
            data.AppendLine("hello :    world");

            var parser = new NvidiaSmiParser();

            var jsonObject = parser.Parse(data.ToString());

            jsonObject["someObject"].Should().NotBeNull("properties with object values should exist");

            jsonObject.Value<string>("hello").Should().Be("world", "property value should return the correct value when coming back from an object property");

            jsonObject = jsonObject["someObject"] as JObject;
            jsonObject.Should().NotBeNull("properties with object values should be JObject types");

            jsonObject.Value<string>("foo").Should().Be("bar", "property lookup should return correct value");
        }

        [Fact]

        public void IncorrectChildObjectStructures()
        {
            var data = new StringBuilder();
            //In this scenario the foo : bar property is going to be treated as a child property to the thing that comes before it.
            //we will convert someObject : someValue into a property someObject_someValue whose value is the child proeprty foo : bar
            data.AppendLine("someObject : someValue");
            data.AppendLine("    foo    : bar    ");

            var parser = new NvidiaSmiParser();

            var jsonObject = parser.Parse(data.ToString());

            jsonObject["someObject_someValue"].Should().NotBeNull("properties value pairs before a child property should be converted into a property itself");

            jsonObject = jsonObject["someObject_someValue"] as JObject;
            jsonObject.Should().NotBeNull("becausee value pairs before a child property should have their value be a jobject.");
            jsonObject.Value<string>("foo").Should().Be("bar", "property names should return the correct value.");
        }

        [Fact]

        public void IncorrectChildObjectStructures_MovingBackUpPropertyHierarchy()
        {
            var data = new StringBuilder();
            //In this scenario the foo : bar property is going to be treated as a child property to the thing that comes before it.
            //we will convert someObject : someValue into a property someObject_someValue whose value is the child proeprty foo : bar
            data.AppendLine("someObject : someValue");
            data.AppendLine("    foo    : bar    ");
            data.AppendLine("anotherObject : someValue");

            var parser = new NvidiaSmiParser();

            var jsonObject = parser.Parse(data.ToString());

            jsonObject.Value<string>("anotherObject").Should().Be("someValue", "proeprty names should return the correct value when coming back up the hierarchy.");
            jsonObject["someObject_someValue"].Should().NotBeNull("properties value pairs before a child property should be converted into a property itself");

            jsonObject = jsonObject["someObject_someValue"] as JObject;

            jsonObject.Should().NotBeNull("becausee value pairs before a child property should have their value be a jobject.");
            jsonObject.Value<string>("foo").Should().Be("bar", "property names should return the correct value.");
        }

        [Fact]
        public void NestedObjectValues()
        {
            var processOutput = new StringBuilder();
            //In this scenario the foo : bar property is going to be treated as a child property to the thing that comes before it.
            //we will convert someObject : someValue into a property someObject_someValue whose value is the child proeprty foo : bar
            processOutput.AppendLine(@"foo");
            processOutput.AppendLine(@"    bar");
            processOutput.AppendLine(@"        bog                        : leg");
            processOutput.AppendLine(@"            Hello                          : world");
            var parser = new NvidiaSmiParser();

            var jsonObject = parser.Parse(processOutput.ToString());

            jsonObject["foo"].Should().NotBeNull("because the first object should exist.");
            jsonObject["foo"]["bar"].Should().NotBeNull("the second object should exist");
            jsonObject["foo"]["bar"]["bog_leg"].Should().NotBeNull("indenting should cause a new object even if this looks like a property value pair");
            jsonObject["foo"]["bar"]["bog_leg"]["Hello"].Should().NotBeNull("properties after fake indenting should exist.");
            jsonObject["foo"]["bar"]["bog_leg"].Value<string>("Hello").Should().Be("world", "properties values after fake indenting should be correct.");
        }

        [Fact]
        public void ErrorWhileNavigatingUpHierarchyEdgeCase1()
        {

            var processOutput = new StringBuilder();
            processOutput.AppendLine("GPU");
            processOutput.AppendLine("    GPU Part Number                       : N/A");
            processOutput.AppendLine("    Inforom Version");
            processOutput.AppendLine("        Image Version                     : G001.0000.03.03");
            processOutput.AppendLine("    GPU Operation Mode");
            processOutput.AppendLine("        Current                           : N/A");

            var parser = new NvidiaSmiParser();
            var jsonObject = parser.Parse(processOutput.ToString());

            jsonObject = jsonObject["GPU"] as JObject;
            jsonObject.Value<string>("GPU Part Number").Should().Be("N/A");
            jsonObject["Inforom Version"].Value<string>("Image Version").Should().Be("G001.0000.03.03");
            jsonObject["GPU Operation Mode"].Value<string>("Current").Should().Be("N/A");
        }

        [Fact]
        public void ErrorWhileNavigatingUpHierarchyEdgeCase2()
        {

            var processOutput = new StringBuilder();
            processOutput.AppendLine("GPU");
            processOutput.AppendLine("    PCI");
            processOutput.AppendLine("        Bus                               : 0x02");
            processOutput.AppendLine("        GPU Link Info");
            processOutput.AppendLine("            PCIe Generation");
            processOutput.AppendLine("                Max                       : 3");
            processOutput.AppendLine("        Bridge Chip");
            processOutput.AppendLine("            Type                          : N/A");
            processOutput.AppendLine("        Replays Since Reset               : 0");
            processOutput.AppendLine("    Fan Speed                             : 0 %");

            var parser = new NvidiaSmiParser();
            var jsonObject = parser.Parse(processOutput.ToString());

            jsonObject = jsonObject["GPU"] as JObject;
            jsonObject.Value<string>("Fan Speed").Should().Be("0 %");
        }
    }
}
