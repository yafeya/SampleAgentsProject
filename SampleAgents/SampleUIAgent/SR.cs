using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using Keysight.KCE.UISamples.Properties;

namespace Keysight.KCE.UISamples
{
    internal class SR
    {
        static ResourceManager mResourceManager = Resources.ResourceManager;

        /// <summary>
        /// Gets the string.
        /// </summary>
        /// <param name="key">The key.</param>w
        /// <returns></returns>
        public static string GetString(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException("key");

            return mResourceManager.GetString(key);
        }
    }
}
