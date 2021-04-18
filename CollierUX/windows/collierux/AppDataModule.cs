using Microsoft.ReactNative.Managed;
using Newtonsoft.Json;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace collierux
{
    [ReactModule("appData", EventEmitterName = "appDataEmitter")]
    class AppDataModule
    {

        [ReactConstant("Pi")]
        public double PI = Math.PI;

        [ReactMethod("getAppSettings")]
        public async Task<string> GetAppSettingsAsync(string settingFileName, string defaultContent)
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile storageFile = null;

            try
            {
                storageFile = await localFolder.GetFileAsync(settingFileName);
            }
            catch (Exception)
            {
                try
                {
                    storageFile = await localFolder.CreateFileAsync(settingFileName);
                }
                catch (Exception e)
                {
                    throw; //not sure what to do here
                }
            }

            try
            {
                var result = await FileIO.ReadTextAsync(storageFile);

                if (result.Length > 0)
                    return result;
            }
            catch (Exception e)
            {
                //no-op.  should mean no such file.  that's file, we will write out the default
                //representation that comes from the UI.
            }

            try
            {
                await FileIO.WriteTextAsync(storageFile, JsonPrettify(defaultContent));
                return await FileIO.ReadTextAsync(storageFile);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }
    }

    /*
    [ReactModule]
    class FancyMath
    {
        [ReactConstant]
        public double E = Math.E;

        [ReactConstant("Pi")]
        public double PI = Math.PI;

        [ReactMethod("add")]
        public double Add(double a, double b)
        {
            return AsyncContext.Run<double>(() => DoStuffAsync(a, b));
        }

        private async Task<double> DoStuffAsync(double a, double b)
        {
            return a + b + 1000;
        }

        [ReactEvent]
        public Action<double> AddEvent { get; set; }

        [ReactMethod]
        public async Task<string> GetHttpResponseAsync(string uri)
        {
            var httpClient = new HttpClient();

            // Send the GET request asynchronously
            var httpResponseMessage = await httpClient.GetAsync(new Uri(uri));

            var content = await httpResponseMessage.Content.ReadAsStringAsync();

            return content;
        }
    }
    */


}
