﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Cryptool.Plugins.PeerToPeerProxy {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "10.0.0.0")]
    internal sealed partial class P2PProxySettings : global::System.Configuration.ApplicationSettingsBase {
        
        private static P2PProxySettings defaultInstance = ((P2PProxySettings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new P2PProxySettings())));
        
        public static P2PProxySettings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool Autoconnect {
            get {
                return ((bool)(this["Autoconnect"]));
            }
            set {
                this["Autoconnect"] = value;
            }
        }
    }
}