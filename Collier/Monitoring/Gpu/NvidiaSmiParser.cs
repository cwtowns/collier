using Collier.IO;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;

namespace Collier.Monitoring.Gpu
{
    public class NvidiaSmiParser : INvidiaSmiParser
    {
        //TODO convert the 4 to a setting in the config file
        private readonly string BlankSpaceIndent = new String(' ', 4);

        public virtual JObject Parse(string data)
        {
            using (var reader = new PeekableStringReaderAdapter(new StringReader(data)))
            {
                JToken currentObject = new JObject();

                int currentIndentCount, lastIndentCount = 0;

                while (true)
                {
                    var line = reader.ReadLine();

                    if (line == null)
                        break;

                    if (line.StartsWith("GPU"))
                        line = line.Replace(":", "_");

                    var separatorIndex = line.IndexOf(":");
                    var isPropertyLine = line.IndexOf(" : ") >= 0;
                    var trimmedLine = line.Trim();

                    //empty lines
                    if (trimmedLine.Length == 0)
                        continue;

                    //comment lines
                    if (trimmedLine.StartsWith("="))
                        continue;

                    currentIndentCount = GetIndentCount(line);

                    //a property with an object structure
                    if (!isPropertyLine)
                    {
                        //this is a child property deeper in the hierarchy, as the object name is at the same indentation as the last property 
                        if (currentIndentCount == lastIndentCount)
                        {
                            currentObject[trimmedLine] = new JObject();
                            currentObject = currentObject[trimmedLine] as JObject;
                            lastIndentCount = currentIndentCount;
                            continue;
                        }

                        //we have moved back up the property list
                        if (currentIndentCount < lastIndentCount)
                        {
                            //for every indent we're moving back we need to iterate the parent twice.
                            //this is because the first parent is a property and we need to find the \
                            //object containing it which will always be the second parent
                            for (int x = 0; x < (lastIndentCount - currentIndentCount) * 2; x++)
                                currentObject = currentObject.Parent;

                            currentObject[trimmedLine] = new JObject();
                            currentObject = currentObject[trimmedLine] as JObject;
                            lastIndentCount = currentIndentCount;
                            continue;
                        }

                        currentObject = currentObject[trimmedLine] = new JObject();
                        lastIndentCount = currentIndentCount;
                        continue;
                    }

                    var propertyName = line.Substring(0, separatorIndex).Trim();
                    var propertyValue = line.Substring(separatorIndex + 1).Trim();

                    string nextLine = null;
                    int nextIndentCount = 0;

                    //moving out of the last object
                    if (currentIndentCount < lastIndentCount)
                    {
                        //for every indent we're moving back we need to iterate the parent twice.
                        //this is because the first parent is a property and we need to find the \
                        //object containing it which will always be the second parent
                        for (int x = 0; x < (lastIndentCount - currentIndentCount) * 2; x++)
                            currentObject = currentObject.Parent;
                    }
                    else if (currentIndentCount > lastIndentCount)
                    {
                        if (currentObject.Count() == 0)
                        {
                            nextLine = reader.PeekLine();
                            nextIndentCount = nextLine == null ? 0 : GetIndentCount(nextLine);

                            if (nextLine != null && nextIndentCount > currentIndentCount)
                            {
                                propertyName = propertyName + "_" + propertyValue;
                                currentObject = currentObject[propertyName] = new JObject();
                                lastIndentCount = nextIndentCount;
                            }
                            else
                            {
                                currentObject[propertyName] = propertyValue;
                                lastIndentCount = currentIndentCount;
                            }

                            continue;
                        }
                    }
                    nextLine = reader.PeekLine();
                    nextIndentCount = nextLine == null ? 0 : GetIndentCount(nextLine);

                    if (nextLine != null && nextIndentCount > currentIndentCount)
                    {
                        propertyName = propertyName + "_" + propertyValue;
                        currentObject = currentObject[propertyName] = new JObject();
                        lastIndentCount = nextIndentCount;
                    }
                    else
                    {
                        currentObject[propertyName] = propertyValue;
                        lastIndentCount = currentIndentCount;
                    }
                }

                while (currentObject.Parent != null)
                    currentObject = currentObject.Parent;

                return currentObject as JObject;
            }
        }

        private int GetIndentCount(string line)
        {
            var chars = line.ToCharArray();
            int count = 0;

            for (int x = 0; x < chars.Length; x++)
            {
                if (chars[x] == ' ')
                    count++;
                else
                    break;
            }

            if (count % BlankSpaceIndent.Length != 0)
                throw new ArgumentOutOfRangeException(nameof(line), "Unexpected indent count.  The amount of leading whitespace is not divisible by 4.  Did the nvidia-smi program change?");

            return count / BlankSpaceIndent.Length;
        }
    }
}
