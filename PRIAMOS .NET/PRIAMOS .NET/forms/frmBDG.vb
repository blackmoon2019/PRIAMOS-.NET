﻿Imports System.Data.SqlClient
Imports System.IO
Imports DevExpress.Utils.Menu
Imports DevExpress.XtraBars.Navigation
Imports DevExpress.XtraEditors
Imports DevExpress.XtraEditors.Repository
Imports DevExpress.XtraGrid.Columns
Imports DevExpress.XtraExport.Xls
Imports DevExpress.XtraGrid.Views.Grid
Imports DevExpress.XtraScheduler.Internal
Imports DevExpress.XtraBars
Imports DevExpress.XtraGrid.Menu
Imports DevExpress.Utils
Imports DevExpress.XtraGrid.Views.Base
Imports DevExpress.XtraEditors.Calendar
Imports DevExpress.XtraEditors.Controls

Public Class frmBDG
    Private sID As String
    Private sAHPBID As String
    Private Ctrl As DevExpress.XtraGrid.Views.Grid.GridView
    Private Frm As DevExpress.XtraEditors.XtraForm
    Public Mode As Byte
    Private Valid As New ValidateControls
    Private Log As New Transactions
    Private FillCbo As New FillCombos
    Private DBQ As New DBQueries
    Private LoadForms As New FormLoader
    Private Cls As New ClearControls
    Private Iam As String
    Private Aam As String
    Friend Class MenuColumnInfo
        Public Sub New(ByVal column As GridColumn)
            Me.Column = column
        End Sub
        Public Column As GridColumn
    End Class
    Public WriteOnly Property ID As String
        Set(value As String)
            sID = value
        End Set
    End Property
    Public WriteOnly Property Scroller As DevExpress.XtraGrid.Views.Grid.GridView
        Set(value As DevExpress.XtraGrid.Views.Grid.GridView)
            Ctrl = value
        End Set
    End Property
    Public WriteOnly Property FormScroller As DevExpress.XtraEditors.XtraForm
        Set(value As DevExpress.XtraEditors.XtraForm)
            Frm = value
        End Set
    End Property
    Private Sub cmdExit_Click(sender As Object, e As EventArgs) Handles cmdExit.Click
        Me.Close()
    End Sub

    Private Sub frmBDG_Load(sender As Object, e As EventArgs) Handles Me.Load
        Dim sSQL As New System.Text.StringBuilder
        txtAam.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric
        txtIam.Properties.Mask.MaskType = DevExpress.XtraEditors.Mask.MaskType.Numeric
        txtAam.Properties.Mask.EditMask = "c" & ProgProps.Decimals
        txtIam.Properties.Mask.EditMask = "c" & ProgProps.Decimals
        'Νομοί
        FillCbo.COU(cboCOU)

        Select Case Mode
            Case FormMode.NewRecord
                dtDTS.EditValue = DateTime.Now
                txtCode.Text = DBQ.GetNextId("BDG")
                cmdAPTAdd.Enabled = False
                cmdAptDel.Enabled = False
                cmdAPTEdit.Enabled = False
                cmdAptRefresh.Enabled = False
            Case FormMode.EditRecord
                If cboCOU.EditValue <> Nothing Then sSQL.AppendLine(" where couid = " & toSQLValueS(cboCOU.EditValue.ToString))
                FillCbo.AREAS(cboAREAS, sSQL)
                Dim myLayoutControls As New List(Of Control)
                myLayoutControls.Add(LayoutControl1) : myLayoutControls.Add(LayoutControl2)
                LoadForms.LoadFormNew(myLayoutControls, "Select * from vw_BDG where id ='" + sID + "'", True)
                Iam = txtIam.EditValue
                Aam = txtAam.EditValue
                LoadForms.LoadDataToGrid(grdAPT, GridView1, "SELECT * FROM VW_APT where bdgid ='" + sID + "' ORDER BY ORD")
        End Select
        Me.CenterToScreen()
        My.Settings.frmBDG = Me.Location
        My.Settings.Save()
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\DEF\APT_def.xml") Then GridView1.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\APT_def.xml", OptionsLayoutBase.FullLayout)
        cmdSave.Enabled = IIf(Mode = FormMode.NewRecord, UserProps.AllowInsert, UserProps.AllowEdit)
    End Sub

    Private Sub frmBDG_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Maximized Then frmMain.XtraTabbedMdiManager1.Dock(Me, frmMain.XtraTabbedMdiManager1)
    End Sub

    Private Sub txtTK_LostFocus(sender As Object, e As EventArgs) Handles txtTK.LostFocus
        FillCbo.ADR(cboADR, ADRsSQL)
    End Sub
    Private Sub cboCOU_EditValueChanged(sender As Object, e As EventArgs) Handles cboCOU.EditValueChanged
        Dim sSQL As New System.Text.StringBuilder
        If cboCOU.EditValue <> Nothing Then sSQL.AppendLine(" where couid = " & toSQLValueS(cboCOU.EditValue.ToString))
        FillCbo.AREAS(cboAREAS, sSQL)
        FillCbo.ADR(cboADR, ADRsSQL)
    End Sub
    Private Sub cboAREAS_EditValueChanged(sender As Object, e As EventArgs) Handles cboAREAS.EditValueChanged
        FillCbo.ADR(cboADR, ADRsSQL)
    End Sub

    Private Sub cmdCboManageCOU_Click(sender As Object, e As EventArgs) Handles cmdCboManageCOU.Click
        Dim form1 As frmGen = New frmGen()
        form1.Text = "Νομοί"
        form1.L1.Text = "Κωδικός"
        form1.L2.Text = "Νομός"
        form1.DataTable = "COU"
        form1.CallerControl = cboCOU
        If cboCOU.EditValue <> Nothing Then form1.ID = cboCOU.EditValue.ToString
        form1.MdiParent = frmMain
        'form1.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        If cboCOU.EditValue <> Nothing Then form1.Mode = FormMode.EditRecord Else form1.Mode = FormMode.NewRecord
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub

    Private Sub cmdCboManageAREAS_Click(sender As Object, e As EventArgs) Handles cmdCboManageAREAS.Click
        Dim form1 As frmGen = New frmGen()
        form1.Text = "Περιοχές"
        form1.L1.Text = "Κωδικός"
        form1.L2.Text = "Περιοχή"
        form1.L3.Text = "Νομός"
        form1.DataTable = "AREAS"
        form1.CallerControl = cboAREAS
        If cboAREAS.EditValue <> Nothing Then form1.ID = cboAREAS.EditValue.ToString
        form1.MdiParent = frmMain
        form1.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        If cboAREAS.EditValue <> Nothing Then form1.Mode = FormMode.EditRecord Else form1.Mode = FormMode.NewRecord
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub
    Private Function ADRsSQL() As System.Text.StringBuilder
        Dim sSQL As New System.Text.StringBuilder
        Dim CouID As String = ""
        Dim AreaID As String = ""
        If cboCOU.EditValue <> Nothing Then CouID = cboCOU.EditValue.ToString
        If cboAREAS.EditValue <> Nothing Then AreaID = cboAREAS.EditValue.ToString
        sSQL.AppendLine("Select id,Name from vw_ADR ")
        If CouID.Length > 0 Or AreaID.Length > 0 Or txtTK.Text.Length > 0 Then sSQL.AppendLine(" where ")
        If CouID.Length > 0 Then sSQL.AppendLine(" couid = " & toSQLValueS(CouID))
        If AreaID.Length > 0 Then
            If CouID.Length > 0 Then sSQL.AppendLine(" AND ")
            sSQL.AppendLine(" AreaID = " & toSQLValueS(AreaID))
        End If
        If txtTK.Text.Length > 0 Then
            If CouID.Length > 0 Or AreaID.Length > 0 Then sSQL.AppendLine(" AND ")
            sSQL.AppendLine(" TK = " & toSQLValue(txtTK))
        End If
        sSQL.AppendLine(" order by name ")
        Return sSQL
    End Function

    Private Sub cmdCboManageADR_Click(sender As Object, e As EventArgs) Handles cmdCboManageADR.Click
        Dim form1 As frmGen = New frmGen()
        form1.Text = "Διευθύνσεις"
        form1.L1.Text = "Κωδικός"
        form1.L2.Text = "Διεύθυνση"
        form1.L3.Text = "Νομός"
        form1.L4.Text = "Περιοχές"
        form1.DataTable = "ADR"
        form1.CallerControl = cboADR
        If cboADR.EditValue <> Nothing Then form1.ID = cboADR.EditValue.ToString
        form1.MdiParent = frmMain

        If cboADR.EditValue <> Nothing Then form1.Mode = FormMode.EditRecord Else form1.Mode = FormMode.NewRecord
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub

    Private Sub NavManage_ElementClick(sender As Object, e As NavElementEventArgs) Handles NavManage.ElementClick
        tabBDG.SelectedTabPage = XtraTabPage2
    End Sub

    Private Sub NavGeneral_ElementClick(sender As Object, e As NavElementEventArgs) Handles NavGeneral.ElementClick
        tabBDG.SelectedTabPage = XtraTabPage1
    End Sub

    Private Sub txtAR_Validated(sender As Object, e As EventArgs) Handles txtAR.Validated
        txtNam.Text = cboADR.Text + " " + txtAR.Text
    End Sub

    Private Sub cboADR_EditValueChanged(sender As Object, e As EventArgs) Handles cboADR.EditValueChanged
        If sender.editvalue <> Nothing Then txtNam.Text = cboADR.Text + " " + txtAR.Text Else txtNam.Text = ""
    End Sub

    Private Sub cmdSave_Click(sender As Object, e As EventArgs) Handles cmdSave.Click
        Dim sResult As Boolean
        Dim sGuid As String
        Try
            If Valid.ValidateForm(LayoutControl1) Then
                Dim myLayoutControls As New List(Of Control)
                myLayoutControls.Add(LayoutControl1) : myLayoutControls.Add(LayoutControl2)
                Select Case Mode
                    Case FormMode.NewRecord
                        sGuid = System.Guid.NewGuid.ToString
                        sResult = DBQ.InsertDataNew(myLayoutControls, "BDG", sGuid, True)
                    Case FormMode.EditRecord
                        sResult = DBQ.UpdateDataNew(myLayoutControls, "BDG", sID, True)
                End Select
                If sResult Then
                    ' Εαν έχει γίνει αλλαγή στην αμοιβή διαχείρισης
                    If Aam <> txtAam.EditValue Then
                        Dim sSQL As String
                        sSQL = "INSERT INTO AAM_H(aam,bdgid,modifiedBy,createdOn) values (" &
                               toSQLValue(txtIam, True) & "," & toSQLValueS(IIf(Mode = FormMode.NewRecord, sGuid, sID)) & "," & toSQLValueS(UserProps.ID.ToString) & ", getdate() )"
                        Using oCmd As New SqlCommand(sSQL, CNDB)
                            oCmd.ExecuteNonQuery()
                        End Using
                        Aam = txtAam.EditValue
                    End If

                    ' Εαν έχει γίνει αλλαγή στην αμοιβή έκδοσης
                    If Iam <> txtIam.EditValue Then
                        Dim sSQL As String
                        sSQL = "INSERT INTO IAM_H(iam,bdgid,modifiedBy,createdOn) values (" &
                               toSQLValue(txtIam, True) & "," & toSQLValueS(IIf(Mode = FormMode.NewRecord, sGuid, sID)) & "," & toSQLValueS(UserProps.ID.ToString) & ", getdate() )"
                        Using oCmd As New SqlCommand(sSQL, CNDB)
                            oCmd.ExecuteNonQuery()
                        End Using
                        Iam = txtIam.EditValue
                    End If

                    dtDTS.EditValue = DateTime.Now
                    txtCode.Text = DBQ.GetNextId("BDG")
                    Dim form As frmScroller = Frm
                    form.LoadRecords("vw_BDG")
                    XtraMessageBox.Show("Η εγγραφή αποθηκέυτηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    If Mode = FormMode.NewRecord Then Mode = FormMode.EditRecord : sID = sGuid
                End If
            End If

        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub txtCode_GotFocus(sender As Object, e As EventArgs) Handles txtCode.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.code"
    End Sub

    Private Sub txtAam_GotFocus(sender As Object, e As EventArgs) Handles txtAam.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.aam"
    End Sub

    Private Sub txtAR_GotFocus(sender As Object, e As EventArgs) Handles txtAR.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.ar"
    End Sub

    Private Sub txtComments_GotFocus(sender As Object, e As EventArgs) Handles txtComments.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.cmt"
    End Sub

    Private Sub txtIam_GotFocus(sender As Object, e As EventArgs) Handles txtIam.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.iam"
    End Sub

    Private Sub txtNam_GotFocus(sender As Object, e As EventArgs) Handles txtNam.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.nam"
    End Sub

    Private Sub txtRmg_GotFocus(sender As Object, e As EventArgs) Handles txtRmg.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.rmg"
    End Sub

    Private Sub txtTK_GotFocus(sender As Object, e As EventArgs) Handles txtTK.GotFocus
        frmMain.bbFields.Caption = "DB Field: ADR.tk"
    End Sub

    Private Sub cboADR_GotFocus(sender As Object, e As EventArgs) Handles cboADR.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.adrid"
    End Sub

    Private Sub cboAREAS_GotFocus(sender As Object, e As EventArgs) Handles cboAREAS.GotFocus
        frmMain.bbFields.Caption = "DB Field: ADR.areaid"
    End Sub

    Private Sub cboCOU_GotFocus(sender As Object, e As EventArgs) Handles cboCOU.GotFocus
        frmMain.bbFields.Caption = "DB Field: ADR.couid"
    End Sub

    Private Sub chkPRD_GotFocus(sender As Object, e As EventArgs) Handles chkPRD.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.prd"
    End Sub

    Private Sub dtDTS_GotFocus(sender As Object, e As EventArgs) Handles dtDTS.GotFocus
        frmMain.bbFields.Caption = "DB Field: BDG.dts"
    End Sub

    Private Sub cmdAam_Click(sender As Object, e As EventArgs) Handles cmdAam.Click
        FillAAM_H()
        FlyoutPanel1.OwnerControl = cmdAam
        FlyoutPanel1.OptionsBeakPanel.AnimationType = DevExpress.Utils.Win.PopupToolWindowAnimation.Fade
        FlyoutPanel1.Options.CloseOnOuterClick = True
        FlyoutPanel1.Options.AnchorType = DevExpress.Utils.Win.PopupToolWindowAnchor.Manual
        FlyoutPanel1.ShowPopup()
    End Sub
    Private Sub FillIAM_H()
        Try
            Dim cmd As SqlCommand = New SqlCommand("SELECT convert(varchar,createdon ,105)+ '  ' + '(' + cast(iam as nvarchar(10)) + ')'  as name
                                                     FROM VW_IAM_H WHERE BDGId = '" & sID & "' order by 1 ", CNDB)
            Dim sdr As SqlDataReader = cmd.ExecuteReader()
            If sdr.HasRows Then
                lstData.Items.Clear()
                lstData.DisplayMember = "name"
                lstData.ValueMember = "name"
                While sdr.Read()
                    lstData.Items.Add(sdr.Item(0).ToString)
                End While
            End If
            sdr.Close()
            sdr = Nothing
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub FillAAM_H()
        Try
            Dim cmd As SqlCommand = New SqlCommand("SELECT convert(varchar,createdon ,105)+ '  ' + '(' + cast(aam as nvarchar(10)) + ')'  as name
                                                     FROM VW_AAM_H WHERE BDGId = '" & sID & "' order by 1 ", CNDB)
            Dim sdr As SqlDataReader = cmd.ExecuteReader()
            If sdr.HasRows Then
                lstData.Items.Clear()
                lstData.DisplayMember = "name"
                lstData.ValueMember = "name"
                While sdr.Read()
                    lstData.Items.Add(sdr.Item(0).ToString)
                End While
            End If
            sdr.Close()
            sdr = Nothing
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub cmdIam_Click(sender As Object, e As EventArgs) Handles cmdIam.Click
        FillIAM_H()
        FlyoutPanel1.OwnerControl = cmdIam
        FlyoutPanel1.OptionsBeakPanel.AnimationType = DevExpress.Utils.Win.PopupToolWindowAnimation.Fade
        FlyoutPanel1.Options.CloseOnOuterClick = True
        FlyoutPanel1.Options.AnchorType = DevExpress.Utils.Win.PopupToolWindowAnchor.Manual
        FlyoutPanel1.ShowPopup()
    End Sub

    Private Sub cmdAPTAdd_Click(sender As Object, e As EventArgs) Handles cmdAPTAdd.Click
        NewApt()
    End Sub

    Private Sub cmdAptRefresh_Click(sender As Object, e As EventArgs) Handles cmdAptRefresh.Click
        AptRefresh()
    End Sub
    Public Sub AptRefresh()
        LoadForms.LoadDataToGrid(grdAPT, GridView1, "SELECT * FROM VW_APT where bdgid ='" + sID + "' ORDER BY ORD")
        If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\DEF\APT_def.xml") Then GridView1.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\APT_def.xml", OptionsLayoutBase.FullLayout)
    End Sub
    Private Sub DeleteApt()
        Dim sSQL As String
        Try
            If GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID") = Nothing Then Exit Sub
            If XtraMessageBox.Show("Θέλετε να διαγραφεί η τρέχουσα εγγραφή?", "PRIAMOS .NET", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then
                sSQL = "DELETE FROM APT WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"

                Using oCmd As New SqlCommand(sSQL, CNDB)
                    oCmd.ExecuteNonQuery()
                End Using
                AptRefresh()
                XtraMessageBox.Show("Η εγγραφή διαγράφηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Private Sub NewApt()
        Dim form1 As frmAPT = New frmAPT()
        form1.Text = "Διαμερίσματα"
        form1.MdiParent = frmMain
        form1.Mode = FormMode.NewRecord
        form1.FormScroller = Me
        form1.BDGID = sID
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub
    Private Sub EditApt()
        Dim form1 As frmAPT = New frmAPT()
        form1.Text = "Διαμερίσματα"
        form1.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
        form1.MdiParent = frmMain
        form1.Mode = FormMode.EditRecord
        form1.FormScroller = Me
        form1.BDGID = sID
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub
    Private Sub grdAPT_KeyDown(sender As Object, e As KeyEventArgs) Handles grdAPT.KeyDown
        Select Case e.KeyCode
            Case Keys.F2 : If UserProps.AllowInsert = True Then NewApt()
            Case Keys.F3 : If UserProps.AllowEdit = True Then EditApt()
            Case Keys.F5 : AptRefresh()
            Case Keys.Delete : If UserProps.AllowDelete = True Then DeleteApt()
        End Select
    End Sub

    Private Sub cmdAptDel_Click(sender As Object, e As EventArgs) Handles cmdAptDel.Click
        DeleteApt()
    End Sub

    Private Sub cmdAPTEdit_Click(sender As Object, e As EventArgs) Handles cmdAPTEdit.Click
        EditApt()
    End Sub

    Private Sub grdAPT_DoubleClick(sender As Object, e As EventArgs) Handles grdAPT.DoubleClick
        EditApt()
    End Sub

    Private Sub GridView1_PopupMenuShowing(sender As Object, e As PopupMenuShowingEventArgs) Handles GridView1.PopupMenuShowing
        If e.MenuType = GridMenuType.Column Then
            Dim menu As DevExpress.XtraGrid.Menu.GridViewColumnMenu = TryCast(e.Menu, GridViewColumnMenu)
            Dim item As New DXEditMenuItem()
            Dim itemColor As New DXEditMenuItem()
            Dim itemSaveView As New DXEditMenuItem()

            'menu.Items.Clear()
            If menu.Column IsNot Nothing Then
                'Για να προσθέσουμε menu item στο Default menu πρέπει πρώτα να προσθέσουμε ένα Repository Item 
                'Υπάρχουν πολλών ειδών Repositorys
                '1st Custom Menu Item
                Dim popRenameColumn As New RepositoryItemTextEdit
                popRenameColumn.Name = "RenameColumn"
                menu.Items.Add(New DXEditMenuItem("Μετονομασία Στήλης", popRenameColumn, AddressOf OnEditValueChanged, Nothing, Nothing, 100, 0))
                item = menu.Items.Item("Μετονομασία Στήλης")
                item.EditValue = menu.Column.GetTextCaption
                item.Tag = menu.Column.AbsoluteIndex
                '2nd Custom Menu Item
                menu.Items.Add(CreateCheckItem("Κλείδωμα Στήλης", menu.Column, Nothing))

                '3rd Custom Menu Item
                Dim popColorsColumn As New RepositoryItemColorEdit
                popColorsColumn.Name = "ColorsColumn"
                menu.Items.Add(New DXEditMenuItem("Χρώμα Στήλης", popColorsColumn, AddressOf OnColumnsColorChanged, Nothing, Nothing, 100, 0))
                itemColor = menu.Items.Item("Χρώμα Στήλης")
                itemColor.EditValue = menu.Column.AppearanceCell.BackColor
                itemColor.Tag = menu.Column.AbsoluteIndex

                '4nd Custom Menu Item
                menu.Items.Add(New DXMenuItem("Αποθήκευση όψης", AddressOf OnSaveView, Nothing, Nothing, Nothing, Nothing))
            End If
        Else
            PopupMenuRows.ShowPopup(Control.MousePosition)
        End If
    End Sub
    'Αλλαγή Χρώματος Στήλης Master
    Private Sub OnColumnsColorChanged(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As DXEditMenuItem = TryCast(sender, DXEditMenuItem)
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView1.Columns(item.Tag).AppearanceCell.BackColor = item.EditValue
    End Sub
    'Αλλαγή Χρώματος Στήλης Master
    Private Sub OnColumnsColorChangedAHPB(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As DXEditMenuItem = TryCast(sender, DXEditMenuItem)
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView2.Columns(item.Tag).AppearanceCell.BackColor = item.EditValue
    End Sub
    'Αποθήκευση όψης Διαμερισμάτων
    Private Sub OnSaveView(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As DXMenuItem = TryCast(sender, DXMenuItem)
        GridView1.SaveLayoutToXml(Application.StartupPath & "\DSGNS\DEF\APT_def.xml", OptionsLayoutBase.FullLayout)
        XtraMessageBox.Show("Η όψη αποθηκεύτηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    'Αποθήκευση όψης Μετρήσεων ωρών
    Private Sub OnSaveViewAHPB(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As DXMenuItem = TryCast(sender, DXMenuItem)
        GridView2.SaveLayoutToXml(Application.StartupPath & "\DSGNS\DEF\AHPB_def.xml", OptionsLayoutBase.FullLayout)
        XtraMessageBox.Show("Η όψη αποθηκεύτηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
    End Sub
    Private Sub OnEditValueChanged(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As New DXEditMenuItem()
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView1.Columns(item.Tag).Caption = item.EditValue
        'MessageBox.Show(item.EditValue.ToString())
    End Sub
    Private Sub OnEditValueChangedAHPB(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As New DXEditMenuItem()
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView2.Columns(item.Tag).Caption = item.EditValue
        'MessageBox.Show(item.EditValue.ToString())
    End Sub
    Private Sub OnCanMoveItemClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim item As DXMenuCheckItem = TryCast(sender, DXMenuCheckItem)
        Dim info As MenuColumnInfo = TryCast(item.Tag, MenuColumnInfo)
        If info Is Nothing Then
            Return
        End If
        info.Column.OptionsColumn.AllowMove = Not item.Checked
    End Sub
    Private Function CreateCheckItem(ByVal caption As String, ByVal column As GridColumn, ByVal image As Image) As DXMenuCheckItem
        Dim item As New DXMenuCheckItem(caption, (Not column.OptionsColumn.AllowMove), image, New EventHandler(AddressOf OnCanMoveItemClick))
        item.Tag = New MenuColumnInfo(column)
        Return item
    End Function

    ' Copy Cell
    Private Sub BarCopyCell_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarCopyCell.ItemClick
        Dim view As GridView = CType(GridView1, GridView)
        If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
            Clipboard.SetText(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString())
        End If
    End Sub
    'Copy All
    Private Sub BarCopyAll_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarCopyAll.ItemClick
        GridView1.OptionsSelection.MultiSelect = True
        GridView1.SelectAll()
        GridView1.CopyToClipboard()
        GridView1.OptionsSelection.MultiSelect = False
    End Sub
    'Copy Row
    Private Sub BarCopyRow_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarCopyRow.ItemClick
        Dim view As GridView = CType(GridView1, GridView)
        If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
            GridView1.OptionsSelection.MultiSelect = True
            GridView1.SelectRow(view.FocusedRowHandle)
            GridView1.CopyToClipboard()
            GridView1.OptionsSelection.MultiSelect = False
        End If
    End Sub

    Private Sub NavHeating_ElementClick(sender As Object, e As NavElementEventArgs) Handles NavHeating.ElementClick
        Dim sSQL As New System.Text.StringBuilder
        tabBDG.SelectedTabPage = XtraTabPage3
        'Τύποι Θέρμανσης
        FillCbo.HTYPES(cboHtypes)
        'Τύποι Boiler
        FillCbo.BTYPES(cboBtypes)
        'Τύποι Καυσίμων
        FillCbo.FTYPES(cboFtypes)
        sSQL.AppendLine("where bdgid ='" + sID + "' and boiler = " & RGTypeHeating.SelectedIndex & " ORDER BY mdt desc")
        'Προηγούμενες μετρήσεις
        FillCbo.BEF_MES(cboBefMes, sSQL)
        If Mode = FormMode.NewRecord Then
            cboHtypes.EditValue = System.Guid.Parse("C331F98B-8504-44CE-9C75-2546B76BAD4E") 'Χωρίς Θέρμανση
        End If
        'cboFtypes.Enabled = False
        'cmdCboManageFtypes.Enabled = False
        'cmdCboManageBtypes.Enabled = False
        'cboBtypes.Enabled = False
        'txtHpc.Enabled = False
        'txtHpb.Enabled = False
        'txtCalH.Enabled = False
        'txtTacH.Enabled = False
        'txtTacB.Enabled = False
        'txtLpcH.Enabled = False
        'txtLpcB.Enabled = False
        'RGBolier.Enabled = False
        'txtCalB.Enabled = False
        'End If
    End Sub
    Private Sub NavHeatingInvoices_ElementClick(sender As Object, e As NavElementEventArgs) Handles NavHeatingInvoices.ElementClick
        tabBDG.SelectedTabPage = XtraTabPage7
        'Προμηθευτές για πετρέλαιο
        FillCbo.SUP(cboOSup)

        'Προμηθευτές για φυσικό αέριο
        FillCbo.SUP(cboFSup)
    End Sub


    Private Sub cmdCboManageBtypes_Click(sender As Object, e As EventArgs)
        Dim form1 As frmGen = New frmGen()
        form1.Text = "Τύποι Boiler"
        form1.L1.Text = "Κωδικός"
        form1.L2.Text = "Τύπος"
        form1.DataTable = "BTYPES"
        form1.CallerControl = cboBtypes
        If cboBtypes.EditValue <> Nothing Then form1.ID = cboBtypes.EditValue.ToString
        form1.MdiParent = frmMain
        form1.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        If cboBtypes.EditValue <> Nothing Then form1.Mode = FormMode.EditRecord Else form1.Mode = FormMode.NewRecord
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub

    Private Sub dtMes_DrawItem(sender As Object, e As CustomDrawDayNumberCellEventArgs)
        ''If (dtMes.EditValue = Nothing) Then
        ''    If (e.Selected = True) Then
        ''If (e.Date = DateTime.Today) Then
        'e.Graphics.DrawRectangle(Pens.Red, e.Bounds)

        ''End If
        'e.Graphics.DrawString(e.Date.Day.ToString(), e.Style.Font, Brushes.Black, e.Bounds, e.Style.GetStringFormat())
        '        e.Handled = True
        ''    End If
        ''End If
    End Sub




    Private Sub cboBefMes_EditValueChanged_1(sender As Object, e As EventArgs) Handles cboBefMes.EditValueChanged
        Dim sSQL As String
        If cboBefMes.EditValue <> Nothing Then
            sSQL = "SELECT * FROM vw_AHPB where bdgid ='" + sID + "' and boiler = " + RGTypeHeating.SelectedIndex.ToString + " and mdt = " + toSQLValueS(CDate(cboBefMes.Text).ToString("yyyyMMdd")) + "  ORDER BY ORD"
            LoadForms.LoadDataToGridForEdit(grdAPTAHPB, GridView2, sSQL)
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\DEF\AHPB_def.xml") Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\AHPB_def.xml", OptionsLayoutBase.FullLayout)
            GridView2.Columns("boiler").OptionsColumn.ReadOnly = True
            GridView2.Columns("nam").OptionsColumn.AllowEdit = False
            cmdDelAHPB.Enabled = True
        Else
            cmdDelAHPB.Enabled = False
        End If
    End Sub

    Private Sub cboBefMes_ButtonClick(sender As Object, e As ButtonPressedEventArgs) Handles cboBefMes.ButtonClick
        If e.Button.Index = 1 Then cboBefMes.EditValue = Nothing
    End Sub

    Private Sub cmdCboManageHtypes_Click(sender As Object, e As EventArgs) Handles cmdCboManageHtypes.Click
        Dim form1 As frmGen = New frmGen()
        form1.Text = "Τύποι Θέρμανσης"
        form1.L1.Text = "Κωδικός"
        form1.L2.Text = "Τύπος"
        form1.DataTable = "HTYPES"
        form1.CallerControl = cboHtypes
        If cboHtypes.EditValue <> Nothing Then form1.ID = cboHtypes.EditValue.ToString
        form1.MdiParent = frmMain
        form1.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        If cboHtypes.EditValue <> Nothing Then form1.Mode = FormMode.EditRecord Else form1.Mode = FormMode.NewRecord
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub

    Private Sub cmdDelAHPB_Click(sender As Object, e As EventArgs) Handles cmdDelAHPB.Click
        Dim sSQL As String
        Dim sBoiler As String
        Try
            If RGTypeHeating.SelectedIndex = 0 Then sBoiler = "Boiler" Else sBoiler = "Θέρμανσης"
            If XtraMessageBox.Show("Θέλετε να διαγραφούν οι ώρες " & sBoiler & " για την ημερομηνία " & cboBefMes.Text & " ?", "PRIAMOS .NET", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then
                sSQL = "DELETE FROM AHPB WHERE bdgID = '" & sID & "' " &
                        " and  mdt = " + toSQLValueS(CDate(cboBefMes.Text).ToString("yyyyMMdd")) &
                        " and boiler = " & RGTypeHeating.SelectedIndex

                Using oCmd As New SqlCommand(sSQL, CNDB)
                    oCmd.ExecuteNonQuery()
                End Using
                Cls.ClearGrid(grdAPTAHPB)
                XtraMessageBox.Show("Η εγγραφή διαγράφηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub cmdAddAHPB_Click(sender As Object, e As EventArgs) Handles cmdAddAHPB.Click
        Try
            If dtMes.EditValue = Nothing Then
                XtraMessageBox.Show("Παρακαλώ επιλέξτε πρώτα ημερομηνία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation)
            Else
                Cls.ClearGrid(grdAPTAHPB)
                Using oCmd As New SqlCommand("CreateAHPB", CNDB)
                    oCmd.CommandType = CommandType.StoredProcedure
                    oCmd.Parameters.AddWithValue("@bdgid", sID)
                    oCmd.Parameters.AddWithValue("@ahpbDT", dtMes.EditValue)
                    oCmd.Parameters.AddWithValue("@bolier", RGTypeHeating.SelectedIndex)
                    oCmd.Parameters.AddWithValue("@modifiedBy", UserProps.ID.ToString)
                    oCmd.ExecuteNonQuery()
                End Using
                LoadForms.LoadDataToGridForEdit(grdAPTAHPB, GridView2, "SELECT * FROM vw_AHPB where bdgid ='" + sID + "' and boiler = " & RGTypeHeating.SelectedIndex & "and mdt = " + toSQLValueS(CDate(dtMes.Text).ToString("yyyyMMdd")) & " ORDER BY ORD")
                If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\DEF\AHPB_def.xml") Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\AHPB_def.xml", OptionsLayoutBase.FullLayout)
                Dim sSQL As New System.Text.StringBuilder
                sSQL.AppendLine("where bdgid ='" + sID + "' and boiler = " & RGTypeHeating.SelectedIndex & " ORDER BY mdt desc")
                'Προηγούμενες μετρήσεις
                FillCbo.BEF_MES(cboBefMes, sSQL)
                cboBefMes.EditValue = Nothing
                GridView2.Columns("boiler").OptionsColumn.ReadOnly = True
                GridView2.Columns("nam").OptionsColumn.AllowEdit = False

            End If

        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub RGTypeHeating_SelectedIndexChanged(sender As Object, e As EventArgs) Handles RGTypeHeating.SelectedIndexChanged
        Dim sSQL As New System.Text.StringBuilder
        sSQL.AppendLine("where bdgid ='" + sID + "' and boiler = " & RGTypeHeating.SelectedIndex & " ORDER BY mdt desc")
        'Προηγούμενες μετρήσεις
        FillCbo.BEF_MES(cboBefMes, sSQL)
        cboBefMes.EditValue = Nothing
        Cls.ClearGrid(grdAPTAHPB)
    End Sub

    Private Sub GridView2_PopupMenuShowing(sender As Object, e As PopupMenuShowingEventArgs) Handles GridView2.PopupMenuShowing
        If e.MenuType = GridMenuType.Column Then
            Dim menu As DevExpress.XtraGrid.Menu.GridViewColumnMenu = TryCast(e.Menu, GridViewColumnMenu)
            Dim item As New DXEditMenuItem()
            Dim itemColor As New DXEditMenuItem()
            Dim itemSaveView As New DXEditMenuItem()

            'menu.Items.Clear()
            If menu.Column IsNot Nothing Then
                'Για να προσθέσουμε menu item στο Default menu πρέπει πρώτα να προσθέσουμε ένα Repository Item 
                'Υπάρχουν πολλών ειδών Repositorys
                '1st Custom Menu Item
                Dim popRenameColumn As New RepositoryItemTextEdit
                popRenameColumn.Name = "RenameColumn"
                menu.Items.Add(New DXEditMenuItem("Μετονομασία Στήλης", popRenameColumn, AddressOf OnEditValueChangedAHPB, Nothing, Nothing, 100, 0))
                item = menu.Items.Item("Μετονομασία Στήλης")
                item.EditValue = menu.Column.GetTextCaption
                item.Tag = menu.Column.AbsoluteIndex
                '2nd Custom Menu Item
                menu.Items.Add(CreateCheckItem("Κλείδωμα Στήλης", menu.Column, Nothing))

                '3rd Custom Menu Item
                Dim popColorsColumn As New RepositoryItemColorEdit
                popColorsColumn.Name = "ColorsColumn"
                menu.Items.Add(New DXEditMenuItem("Χρώμα Στήλης", popColorsColumn, AddressOf OnColumnsColorChangedAHPB, Nothing, Nothing, 100, 0))
                itemColor = menu.Items.Item("Χρώμα Στήλης")
                itemColor.EditValue = menu.Column.AppearanceCell.BackColor
                itemColor.Tag = menu.Column.AbsoluteIndex

                '4nd Custom Menu Item
                menu.Items.Add(New DXMenuItem("Αποθήκευση όψης", AddressOf OnSaveViewAHPB, Nothing, Nothing, Nothing, Nothing))
            End If
        Else
            PopupMenuRows.ShowPopup(Control.MousePosition)
        End If
    End Sub

    Private Sub GridView2_RowUpdated(sender As Object, e As RowObjectEventArgs) Handles GridView2.RowUpdated
        Dim sSQL As String
        Dim mes As Decimal
        Dim mesB As Decimal
        Dim Dif As Decimal
        Try
            If GridView2.GetRowCellValue(GridView2.FocusedRowHandle, "ID") = Nothing Then Exit Sub
            If GridView2.GetRowCellValue(GridView2.FocusedRowHandle, "mes") Is DBNull.Value Then Exit Sub
            If GridView2.GetRowCellValue(GridView2.FocusedRowHandle, "mes") Is DBNull.Value Then Exit Sub
            mes = GridView2.GetRowCellValue(GridView2.FocusedRowHandle, "mes")
            mesB = GridView2.GetRowCellValue(GridView2.FocusedRowHandle, "mesB")
            Dif = mes - mesB
            GridView2.SetRowCellValue(GridView2.FocusedRowHandle, "mesDif", Dif)
            sSQL = "UPDATE  AHPB SET MES = " & toSQLValueS(mes, True) &
                    ",MESB = " & toSQLValueS(mesB, True) &
                    ",MESDIF = " & toSQLValueS(Dif, True) &
                    " WHERE ID = '" & GridView2.GetRowCellValue(GridView2.FocusedRowHandle, "ID").ToString & "'"

            Using oCmd As New SqlCommand(sSQL, CNDB)
                oCmd.ExecuteNonQuery()
            End Using
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub

    Private Sub cmdCboManageFtypes_Click(sender As Object, e As EventArgs) Handles cmdCboManageFtypes.Click
        Dim form1 As frmGen = New frmGen()
        form1.Text = "Τύποι Καυσίμων"
        form1.L1.Text = "Κωδικός"
        form1.L2.Text = "Τύπος"
        form1.DataTable = "FTYPES"
        form1.CallerControl = cboFtypes
        If cboFtypes.EditValue <> Nothing Then form1.ID = cboFtypes.EditValue.ToString
        form1.MdiParent = frmMain
        form1.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        form1.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
        If cboFtypes.EditValue <> Nothing Then form1.Mode = FormMode.EditRecord Else form1.Mode = FormMode.NewRecord
        frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(form1), New Point(CInt(form1.Parent.ClientRectangle.Width / 2 - form1.Width / 2), CInt(form1.Parent.ClientRectangle.Height / 2 - form1.Height / 2)))
        form1.Show()
    End Sub


    'ΘΕΡΜΑΝΣΗ
    '    Private Sub cboHtypes_EditValueChanged(sender As Object, e As EventArgs) Handles cboHtypes.EditValueChanged

    '        Select Case cboHtypes.EditValue
    '            'Αυτονομία με σταθερό πάγιο
    '            Case System.Guid.Parse("1879D79E-6071-4C1B-A9A4-0AEC70EB8025")
    '                cboBtypes.Enabled = True
    '                cmdCboManageBtypes.Enabled = True
    '                cboFtypes.Enabled = True
    '                txtHpc.Enabled = True
    '                RGBolier.Enabled = True
    '            'Αυτονομία με χρήση Fi
    '            Case System.Guid.Parse("64FE370B-9499-4263-861E-E1820E0ED9CD")
    '                cboBtypes.Enabled = True
    '                cmdCboManageBtypes.Enabled = True
    '                cboFtypes.Enabled = True
    '                txtHpc.Enabled = False
    '                RGBolier.Enabled = False
    '            'Κεντρική Θέρμανση
    '            Case System.Guid.Parse("D29BAB0F-F94C-428F-A699-102EE9EF0BC2")
    '                cboFtypes.Enabled = True
    '                txtHpc.Enabled = False
    '                RGBolier.Enabled = False
    '            Case Else
    '                cboBtypes.Enabled = False
    '                cboBtypes.EditValue = Nothing
    '                cboBtypes.EditValue = ""
    '                cboBtypes.Text = ""
    '                cmdCboManageBtypes.Enabled = False
    '                cboFtypes.Enabled = False
    '                txtHpc.Enabled = False
    '                RGBolier.Enabled = False
    '        End Select
    '    End Sub
    '    'BOILER
    '    Private Sub cboBtypes_EditValueChanged(sender As Object, e As EventArgs) Handles cboBtypes.EditValueChanged
    '        Select Case cboBtypes.EditValue
    '            'Με Σταθερό Πάγιο
    '            Case System.Guid.Parse("9E155093-4E5E-42AB-AFEB-57239B8E25F0")
    '                txtHpb.Enabled = True
    '                RGBolier.Enabled = True
    '            'Με Χρήση Fi
    '            Case System.Guid.Parse("19E83D91-E350-47DA-9BE0-80AEF55AC70A")
    '                RGBolier.Enabled = True
    '            Case Else
    '                txtHpb.Enabled = False
    '                RGBolier.Enabled = False
    '        End Select
    '    End Sub
    '    'ΤΥΠΟΣ ΚΑΥΣΙΜΟΥ
    '    Private Sub cboFtypes_EditValueChanged(sender As Object, e As EventArgs) Handles cboFtypes.EditValueChanged
    '        Select Case cboFtypes.EditValue
    '            'Πετρέλαιο
    '            Case System.Guid.Parse("AA662AEB-2B3B-4189-8253-A904669E7BCB")
    '                If cboHtypes.EditValue = System.Guid.Parse("D29BAB0F-F94C-428F-A699-102EE9EF0BC2") Or
    '                   cboHtypes.EditValue = System.Guid.Parse("1879D79E-6071-4C1B-A9A4-0AEC70EB8025") Or
    '                   cboHtypes.EditValue = System.Guid.Parse("64FE370B-9499-4263-861E-E1820E0ED9CD") Then
    '                    txtTacH.Enabled = True
    '                    txtLpcH.Enabled = True
    '                Else
    '                    txtTacH.Enabled = False
    '                    txtLpcH.Enabled = False
    '                End If
    '            Case Else
    '                txtTacH.Enabled = False
    '        End Select
    '    End Sub

    '    Private Sub RGBolier_SelectedIndexChanged(sender As Object, e As EventArgs) Handles RGBolier.SelectedIndexChanged
    '        'Ξεχωριστός
    '        If RGBolier.SelectedIndex = 1 Then
    '            If cboFtypes.EditValue = System.Guid.Parse("AA662AEB-2B3B-4189-8253-A904669E7BCB") Then
    '                If cboBtypes.EditValue = System.Guid.Parse("9E155093-4E5E-42AB-AFEB-57239B8E25F0") Or cboBtypes.EditValue = System.Guid.Parse("19E83D91-E350-47DA-9BE0-80AEF55AC70A") Then
    '                    txtLpcH.Enabled = True
    '                Else
    '                    txtLpcH.Enabled = False
    '                End If
    '            End If
    '            'Boiler
    '            Select Case cboBtypes.EditValue
    '            'Με Σταθερό Πάγιο - 'Με Χρήση Fi
    '                    Case System.Guid.Parse("9E155093-4E5E-42AB-AFEB-57239B8E25F0"), System.Guid.Parse("19E83D91-E350-47DA-9BE0-80AEF55AC70A")
    '                        If cboFtypes.EditValue = System.Guid.Parse("AA662AEB-2B3B-4189-8253-A904669E7BCB") Then
    '                            txtTacB.Enabled = True
    '                        End If
    '                    Case Else
    '                        txtHpb.Enabled = False
    '                        RGBolier.Enabled = False
    '                        txtTacB.Enabled = False
    '                End Select
    '            Else
    '                txtTacB.Enabled = False
    '        End If
    '    End Sub
End Class