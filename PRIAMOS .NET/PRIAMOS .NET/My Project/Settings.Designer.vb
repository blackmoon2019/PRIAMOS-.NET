﻿'------------------------------------------------------------------------------
' <auto-generated>
'     This code was generated by a tool.
'     Runtime Version:4.0.30319.42000
'
'     Changes to this file may cause incorrect behavior and will be lost if
'     the code is regenerated.
' </auto-generated>
'------------------------------------------------------------------------------

Option Strict On
Option Explicit On


Namespace My
    
    <Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute(),  _
     Global.System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.8.1.0"),  _
     Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)>  _
    Partial Friend NotInheritable Class MySettings
        Inherits Global.System.Configuration.ApplicationSettingsBase
        
        Private Shared defaultInstance As MySettings = CType(Global.System.Configuration.ApplicationSettingsBase.Synchronized(New MySettings()),MySettings)
        
#Region "My.Settings Auto-Save Functionality"
#If _MyType = "WindowsForms" Then
    Private Shared addedHandler As Boolean

    Private Shared addedHandlerLockObject As New Object

    <Global.System.Diagnostics.DebuggerNonUserCodeAttribute(), Global.System.ComponentModel.EditorBrowsableAttribute(Global.System.ComponentModel.EditorBrowsableState.Advanced)> _
    Private Shared Sub AutoSaveSettings(sender As Global.System.Object, e As Global.System.EventArgs)
        If My.Application.SaveMySettingsOnExit Then
            My.Settings.Save()
        End If
    End Sub
#End If
#End Region
        
        Public Shared ReadOnly Property [Default]() As MySettings
            Get
                
#If _MyType = "WindowsForms" Then
               If Not addedHandler Then
                    SyncLock addedHandlerLockObject
                        If Not addedHandler Then
                            AddHandler My.Application.Shutdown, AddressOf AutoSaveSettings
                            addedHandler = True
                        End If
                    End SyncLock
                End If
#End If
                Return defaultInstance
            End Get
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("30")>  _
        Public Property Records() As String
            Get
                Return CType(Me("Records"),String)
            End Get
            Set
                Me("Records") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("")>  _
        Public Property CurrentView() As String
            Get
                Return CType(Me("CurrentView"),String)
            End Get
            Set
                Me("CurrentView") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmUsers() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmUsers"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmUsers") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmMailSettings() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmMailSettings"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmMailSettings") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmPermissions() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmPermissions"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmPermissions") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("False")>  _
        Public Property UNSave() As Boolean
            Get
                Return CType(Me("UNSave"),Boolean)
            End Get
            Set
                Me("UNSave") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("00000000-0000-0000-0000-000000000000")>  _
        Public Property UN() As Global.System.Guid
            Get
                Return CType(Me("UN"),Global.System.Guid)
            End Get
            Set
                Me("UN") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmBDG() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmBDG"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmBDG") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmGen() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmGen"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmGen") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmCustomers() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmCustomers"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmCustomers") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmAPT() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmAPT"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmAPT") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("1.0.0.5")>  _
        Public Property ExeVer() As String
            Get
                Return CType(Me("ExeVer"),String)
            End Get
            Set
                Me("ExeVer") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("1.0.0.5")>  _
        Public Property DbVer() As String
            Get
                Return CType(Me("DbVer"),String)
            End Get
            Set
                Me("DbVer") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmParameters() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmParameters"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmParameters") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmTecnicalSupport() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmTecnicalSupport"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmTecnicalSupport") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute()>  _
        Public Property ConString() As Global.System.Collections.Specialized.StringCollection
            Get
                Return CType(Me("ConString"),Global.System.Collections.Specialized.StringCollection)
            End Get
            Set
                Me("ConString") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute()>  _
        Public Property ConStringServers() As Global.System.Collections.Specialized.StringCollection
            Get
                Return CType(Me("ConStringServers"),Global.System.Collections.Specialized.StringCollection)
            End Get
            Set
                Me("ConStringServers") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("\\192.168.1.52\TempFiles\")>  _
        Public Property SERVER_PATH() As String
            Get
                Return CType(Me("SERVER_PATH"),String)
            End Get
            Set
                Me("SERVER_PATH") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmEXP() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmEXP"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmEXP") = value
            End Set
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmIEP() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmIEP"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmIEP") = value
            End Set
        End Property
        
        <Global.System.Configuration.ApplicationScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.SpecialSettingAttribute(Global.System.Configuration.SpecialSetting.ConnectionString),  _
         Global.System.Configuration.DefaultSettingValueAttribute("Data Source=192.168.1.52,1433;Initial Catalog=Priamos_NET;Persist Security Info=T"& _ 
            "rue;User ID=sa;Password=12pri2020#$")>  _
        Public ReadOnly Property Priamos_NETConnectionStringRemote() As String
            Get
                Return CType(Me("Priamos_NETConnectionStringRemote"),String)
            End Get
        End Property
        
        <Global.System.Configuration.UserScopedSettingAttribute(),  _
         Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
         Global.System.Configuration.DefaultSettingValueAttribute("0, 0")>  _
        Public Property frmINH() As Global.System.Drawing.Point
            Get
                Return CType(Me("frmINH"),Global.System.Drawing.Point)
            End Get
            Set
                Me("frmINH") = value
            End Set
        End Property
    End Class
End Namespace

Namespace My
    
    <Global.Microsoft.VisualBasic.HideModuleNameAttribute(),  _
     Global.System.Diagnostics.DebuggerNonUserCodeAttribute(),  _
     Global.System.Runtime.CompilerServices.CompilerGeneratedAttribute()>  _
    Friend Module MySettingsProperty
        
        <Global.System.ComponentModel.Design.HelpKeywordAttribute("My.Settings")>  _
        Friend ReadOnly Property Settings() As Global.PRIAMOS.NET.My.MySettings
            Get
                Return Global.PRIAMOS.NET.My.MySettings.Default
            End Get
        End Property
    End Module
End Namespace
