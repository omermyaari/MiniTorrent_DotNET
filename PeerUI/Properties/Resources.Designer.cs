﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PeerUI.Properties {
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
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("PeerUI.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to MyConfig.xml.
        /// </summary>
        internal static string configFileName {
            get {
                return ResourceManager.GetString("configFileName", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Connected..
        /// </summary>
        internal static string connectedString {
            get {
                return ResourceManager.GetString("connectedString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 192.168.0.0.
        /// </summary>
        internal static string defaultIP {
            get {
                return ResourceManager.GetString("defaultIP", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Config file does not exist, please fill the settings..
        /// </summary>
        internal static string errorConfigFileNotExist {
            get {
                return ResourceManager.GetString("errorConfigFileNotExist", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Directory not found..
        /// </summary>
        internal static string errorDirectoryNotFound {
            get {
                return ResourceManager.GetString("errorDirectoryNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Couldnt request file from main server - endpoint not found..
        /// </summary>
        internal static string errorEndpointNotFound {
            get {
                return ResourceManager.GetString("errorEndpointNotFound", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: File name cannot be empty..
        /// </summary>
        internal static string errorFileNameEmpty {
            get {
                return ResourceManager.GetString("errorFileNameEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Couldnt request file from main server - timeout..
        /// </summary>
        internal static string errorFileRequestTimeout {
            get {
                return ResourceManager.GetString("errorFileRequestTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Couldn&apos;t generate a sign in request, check your settings..
        /// </summary>
        internal static string errorGenerateSignIn {
            get {
                return ResourceManager.GetString("errorGenerateSignIn", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Couldn&apos;t sign in to main server - timeout..
        /// </summary>
        internal static string errorSignInTimeout {
            get {
                return ResourceManager.GetString("errorSignInTimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Error: Couldn&apos;t sign in to main server - username or password is incorrect..
        /// </summary>
        internal static string errorUsernamePassword {
            get {
                return ResourceManager.GetString("errorUsernamePassword", resourceCulture);
            }
        }
    }
}
