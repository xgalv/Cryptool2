﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.239
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cryptool.PluginBase.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Cryptool.PluginBase.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Can&apos;t find localization key {0} in class {1}!.
        /// </summary>
        public static string Can_t_find_localization_key {
            get {
                return ResourceManager.GetString("Can_t_find_localization_key", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error trying to lookup localization key {0}! Message: {1}.
        /// </summary>
        public static string Error_trying_to_lookup_localization_key {
            get {
                return ResourceManager.GetString("Error_trying_to_lookup_localization_key", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to No data available yet!.
        /// </summary>
        public static string No_data_available_yet {
            get {
                return ResourceManager.GetString("No_data_available_yet", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error creating plugin type: {0}. Exception is: {1}.
        /// </summary>
        public static string plugin_extension_create_object {
            get {
                return ResourceManager.GetString("plugin_extension_create_object", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error getting description from plugin: {0}. Exception is: {1}.
        /// </summary>
        public static string plugin_extension_error_get_description {
            get {
                return ResourceManager.GetString("plugin_extension_error_get_description", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error getting image from plugin: {0}. Exception is: {1}.
        /// </summary>
        public static string plugin_extension_error_get_image {
            get {
                return ResourceManager.GetString("plugin_extension_error_get_image", resourceCulture);
            }
        }
    }
}
