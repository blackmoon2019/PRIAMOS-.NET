﻿Public Class frmSplash
    Sub New
        InitializeComponent()
        Me.labelCopyright.Text = "Copyright © 2020-" & DateTime.Now.Year.ToString()
        Me.lblVer.Text = "V " & My.Application.Info.Version.ToString

    End Sub

    Public Overrides Sub ProcessCommand(ByVal cmd As System.Enum, ByVal arg As Object)
        MyBase.ProcessCommand(cmd, arg)
    End Sub

    Public Enum SplashScreenCommand
        SomeCommandId
    End Enum
End Class
