using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Markup;

namespace Keysight.KCE.UISamples.Utilities
{
    /// <summary>
    /// 
    /// </summary>
    [MarkupExtensionReturnType(typeof(object)), ContentProperty("Key")]
    public class ResExtension : MarkupExtension
    {
        #region ... Variables  ...

        #endregion ...Variables...

        #region ... Events     ...

        #endregion ...Events...

        #region ... Constructor...
        /// <summary>
        /// Initializes a new instance of the <see cref="ResExtension"/> class.
        /// </summary>
        public ResExtension()
        {

        }
        /// <summary>
        /// Initializes a new instance of the <see cref="ResExtension"/> class.
        /// </summary>
        /// <param name="key">The key.</param>
        public ResExtension(string key)
        {
            Key = key;
        }
        #endregion ...Constructor...

        #region ... Properties ...
        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; set; }
        #endregion ...Properties...

        #region ... Methods    ...
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        object GetValue(string key)
        {
            if (string.IsNullOrEmpty(key))
                return "Resource key is empty.";

            var value = SR.GetString(key);
            if (string.IsNullOrEmpty(value))
                return string.Format("Can't find resource value by key:'{0}'.", key);
            else
                return value;
        }
        #endregion ...Methods...

        #region ... Interfaces ...
        /// <summary>
        /// When implemented in a derived class, returns an object that is set as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">Object that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (service != null)
            {
                // Design time?
            }

            return GetValue(Key);
        }
        #endregion ...Interfaces...
    }
}
