using Microsoft.ReactNative.Managed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace collierux
{
    /// <summary>
    /// Native implementation for random guid generation since crypto does not exist in Release configurations
    /// </summary>
    [ReactModule("nativeRandom", EventEmitterName = "nativeRandomEmitter")]
    class NativeRandom
    {
        [ReactMethod("generateGuid")]
        public string GenerateGuid()
        {
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);

                return new Guid(bytes).ToString();
            }
        }
    }
}
