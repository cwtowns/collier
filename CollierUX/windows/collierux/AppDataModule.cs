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
    /// <summary>
    /// Reads configuration data out of the common UWP data store location.  
    /// This is used for storing front end configuraiton data for individual
    /// user override without requiring recompilation.  For example, users can
    /// change the threshold settings for a good / caution / danger state toggle,
    /// or they can change the colors we use when we're in those states.  
    /// 
    /// The files can be reset to stock by deleting them and re-running the application.
    /// We will persist out the application defaults when the file does not exist or has
    /// no contents.  
    /// </summary>
    [ReactModule("appData", EventEmitterName = "appDataEmitter")]
    class AppDataModule
    {
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
}
