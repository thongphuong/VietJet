﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ClientMonitorLibrary.VietjetAPI_AppView {
    using System.Runtime.Serialization;
    using System;
    
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="HeartBeatResponse", Namespace="http://schemas.datacontract.org/2004/07/VietJetAirAPI.Services")]
    [System.SerializableAttribute()]
    public partial class HeartBeatResponse : object, System.Runtime.Serialization.IExtensibleDataObject, System.ComponentModel.INotifyPropertyChanged {
        
        [System.NonSerializedAttribute()]
        private System.Runtime.Serialization.ExtensionDataObject extensionDataField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private ClientMonitorLibrary.VietjetAPI_AppView.OrderCode[] OrderField;
        
        [System.Runtime.Serialization.OptionalFieldAttribute()]
        private string ResultCodeField;
        
        [global::System.ComponentModel.BrowsableAttribute(false)]
        public System.Runtime.Serialization.ExtensionDataObject ExtensionData {
            get {
                return this.extensionDataField;
            }
            set {
                this.extensionDataField = value;
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public ClientMonitorLibrary.VietjetAPI_AppView.OrderCode[] Order {
            get {
                return this.OrderField;
            }
            set {
                if ((object.ReferenceEquals(this.OrderField, value) != true)) {
                    this.OrderField = value;
                    this.RaisePropertyChanged("Order");
                }
            }
        }
        
        [System.Runtime.Serialization.DataMemberAttribute()]
        public string ResultCode {
            get {
                return this.ResultCodeField;
            }
            set {
                if ((object.ReferenceEquals(this.ResultCodeField, value) != true)) {
                    this.ResultCodeField = value;
                    this.RaisePropertyChanged("ResultCode");
                }
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Runtime.Serialization", "4.0.0.0")]
    [System.Runtime.Serialization.DataContractAttribute(Name="OrderCode", Namespace="http://schemas.datacontract.org/2004/07/VietJetAirAPI.Utility")]
    public enum OrderCode : int {
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        None = 0,
        
        [System.Runtime.Serialization.EnumMemberAttribute()]
        Capture_Screen = 1,
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName="VietjetAPI_AppView.IAppView")]
    public interface IAppView {
        
        [System.ServiceModel.OperationContractAttribute(Action="http://tempuri.org/IAppView/HeartBeat", ReplyAction="http://tempuri.org/IAppView/HeartBeatResponse")]
        ClientMonitorLibrary.VietjetAPI_AppView.HeartBeatResponse HeartBeat(string ComputerName, string AppName);
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface IAppViewChannel : ClientMonitorLibrary.VietjetAPI_AppView.IAppView, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class AppViewClient : System.ServiceModel.ClientBase<ClientMonitorLibrary.VietjetAPI_AppView.IAppView>, ClientMonitorLibrary.VietjetAPI_AppView.IAppView {
        
        public AppViewClient() {
        }
        
        public AppViewClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public AppViewClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AppViewClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public AppViewClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        public ClientMonitorLibrary.VietjetAPI_AppView.HeartBeatResponse HeartBeat(string ComputerName, string AppName) {
            return base.Channel.HeartBeat(ComputerName, AppName);
        }
    }
}