﻿Imports System.Windows.Forms
Imports System.Data.SqlClient
Imports DevExpress.XtraSplashScreen
Imports DevExpress.XtraEditors
Imports DevExpress.XtraEditors.Repository
Imports DevExpress.XtraBars
Imports DevExpress.XtraGrid.Views.Grid
Imports DevExpress.Utils.Menu
Imports DevExpress.XtraGrid.Columns
Imports DevExpress.XtraGrid.Menu
Imports DevExpress.XtraPrinting
Imports DevExpress.Export
Imports DevExpress.XtraGrid.Views.Base
Imports DevExpress.Utils
Imports DevExpress.XtraEditors.Controls
Imports DevExpress.XtraGrid.Localization

Public Class frmScroller
    Private myConn As SqlConnection
    Private myCmd As SqlCommand
    Private myReader As SqlDataReader
    Private sDataTable As String
    Private sDataDetail As String
    Private CurrentView As String
    Private ReadXml As New XmlUpdateFromDB
    Private LoadForms As New FormLoader

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub
    'Private settings = System.Configuration.ConfigurationManager.AppSettings
    Private Sub frmScroller_Load(sender As Object, e As EventArgs) Handles Me.Load
        'Λίστα με τιμές για TOP RECORDS
        LoadComboRecordValues()
        popSaveAsView.EditValue = BarViews.EditValue
        If BarViews.EditValue = "" Then popSaveView.Enabled = False : popDeleteView.Enabled = False
        'Παίρνω το όνομα της όψης για τον συγκεκριμένο χρήστη και για τον συγκεκριμένο πίνακα 
        GetCurrentView(True)
        'Φόρτωση Εγγραφών
        LoadRecords()
        'Φόρτωση Σχεδίων στην Λίστα βάση επιλογής από το μενού
        'LoadViews()
        'Φορτώνει όλες τις ονομασίες των στηλών από τον SQL. Από το πεδίο Description
        LoadForms.LoadColumnDescriptionNames(grdMain, GridView1, , sDataTable)

        GridLocalizer.Active = New GreekGridLocalizer()
        'Localizer.Active = New GermanEditorsLocalizer()

        'Κρύψιμο Στηλών
        'HideColumns(GridView1, "ID")
        'Δικαιώματα
        BarNewRec.Enabled = UserProps.AllowInsert
        BarDelete.Enabled = UserProps.AllowDelete
        BarEdit.Enabled = UserProps.AllowEdit
    End Sub

    'Λίστα με τιμές για TOP RECORDS
    'Φόρτωση Λίστας με εγγραφές 
    Private Sub LoadComboRecordValues()
        CType(BarRecords.Edit, RepositoryItemComboBox).Items.Add("30")
        CType(BarRecords.Edit, RepositoryItemComboBox).Items.Add("200")
        CType(BarRecords.Edit, RepositoryItemComboBox).Items.Add("1000")
        CType(BarRecords.Edit, RepositoryItemComboBox).Items.Add("10000")
        CType(BarRecords.Edit, RepositoryItemComboBox).Items.Add("ALL")
        BarRecords.EditValue = My.Settings.Records
    End Sub
    'Φόρτωση όψεων Per User στο Combo
    Private Sub LoadViews()
        Try
            BarViews.EditValue = ""
            'Εαν δεν υπάρχει Default Σχέδιο δημιουργεί
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml") = False Then
                GridView1.OptionsLayout.LayoutVersion = "v1"
                GridView1.SaveLayoutToXml(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml", OptionsLayoutBase.FullLayout)
            End If
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\DEF\" & sDataDetail & "_def.xml") = False Then
                If sDataDetail <> "" Then GridView2.SaveLayoutToXml(Application.StartupPath & "\DSGNS\DEF\" & sDataDetail & "_def.xml", OptionsLayoutBase.FullLayout)
            End If

            'Εαν δεν υπάρχει Folder Σχεδίου για το συγκεκριμένο πίνακα δημιουργεί
            If My.Computer.FileSystem.DirectoryExists(Application.StartupPath & "\DSGNS\" & sDataTable) = False Then _
                My.Computer.FileSystem.CreateDirectory(Application.StartupPath & "\DSGNS\" & sDataTable)

            'Εαν δεν υπάρχει Folder Σχεδίου για το Detail πίνακα δημιουργεί
            If My.Computer.FileSystem.DirectoryExists(Application.StartupPath & "\DSGNS\" & sDataDetail) = False Then _
                My.Computer.FileSystem.CreateDirectory(Application.StartupPath & "\DSGNS\" & sDataDetail)

            CType(BarViews.Edit, RepositoryItemComboBox).Items.Clear()
            'Ψάχνει όλα τα σχέδια  του συκεκριμένου χρήστη για τον συγκεκριμένο πίνακα
            Dim files() As String = IO.Directory.GetFiles(Application.StartupPath & "\DSGNS\" & sDataTable, "*_" & UserProps.Code & "*")
            For Each sFile As String In files
                CType(BarViews.Edit, RepositoryItemComboBox).Items.Add(System.IO.Path.GetFileName(sFile))
            Next
            BarViews.EditValue = CurrentView
            If CurrentView = "" Then
                'grdMain.DefaultView.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml")
                GridView1.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml", OptionsLayoutBase.FullLayout)
                If sDataDetail <> "" Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\" & sDataDetail & "_def.xml", OptionsLayoutBase.FullLayout)
            Else
                'grdMain.DefaultView.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue)
                GridView1.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
                If sDataDetail <> "" Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    'Διαγραφη Εγγραφής
    Private Sub DeleteRecord()
        Dim sSQL As String
        Dim sSQL2 As String
        Try
            If GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID") = Nothing Then Exit Sub
            If XtraMessageBox.Show("Θέλετε να διαγραφεί η τρέχουσα εγγραφή?", "PRIAMOS .NET", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then
                Select Case sDataTable
                    Case "vw_USR" : sSQL = "DELETE FROM USR WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_MAILS" : sSQL = "DELETE FROM MAILS WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_RIGHTS" : sSQL = "DELETE FROM RIGHTS WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                        sSQL2 = "DELETE FROM FORM_RIGHTS WHERE RID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_BDG" : sSQL = "DELETE FROM BDG WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_COU" : sSQL = "DELETE FROM COU WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_AREAS" : sSQL = "DELETE FROM AREAS WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_ADR" : sSQL = "DELETE FROM ADR WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_DOY" : sSQL = "DELETE FROM DOY WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_PRF" : sSQL = "DELETE FROM PRF WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_CCT" : sSQL = "DELETE FROM CCT WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_FTYPES" : sSQL = "DELETE FROM FTYPES WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_PRM" : sSQL = "DELETE FROM PRM WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_CALC_TYPES" : sSQL = "DELETE FROM CALC_TYPES WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_MLC" : sSQL = "DELETE FROM MLC WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_TECH_CAT" : sSQL = "DELETE FROM TECH_CAT WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_TECH_SUP" : sSQL = "DELETE FROM TECH_SUP WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_CALC_CAT" : sSQL = "DELETE FROM CALC_CAT WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_EXP" : sSQL = "DELETE FROM EXP WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                    Case "vw_INH" : sSQL = "DELETE FROM INH WHERE ID = '" & GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString & "'"
                End Select

                Using oCmd As New SqlCommand(sSQL, CNDB)
                    oCmd.ExecuteNonQuery()
                End Using
                If sSQL2 <> "" Then
                    Using oCmd As New SqlCommand(sSQL2, CNDB)
                        oCmd.ExecuteNonQuery()
                    End Using
                End If
                LoadRecords()
                XtraMessageBox.Show("Η εγγραφή διαγράφηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    Public WriteOnly Property DataTable As String
        Set(value As String)
            sDataTable = value
        End Set
    End Property
    Public WriteOnly Property DataDetail As String
        Set(value As String)
            sDataDetail = value
        End Set
    End Property
    'Επιλογή όψης
    Private Sub BarViews_EditValueChanged(sender As Object, e As EventArgs) Handles BarViews.EditValueChanged
        Try
            popSaveAsView.EditValue = BarViews.EditValue
            If BarViews.EditValue <> "" Then
                'grdMain.DefaultView.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue)
                GridView1.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
                If sDataDetail <> "" Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
                CurrentView = BarViews.EditValue
                popSaveView.Enabled = True
                popDeleteView.Enabled = True
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    'Κλείσιμο Φόρμας
    Private Sub frmScroller_Closed(sender As Object, e As EventArgs) Handles Me.Closed
        Try
            'Παίρνω το όνομα της όψης για τον συγκεκριμένο χρήστη και για τον συγκεκριμένο πίνακα και το αποθηκεύω στην βάση
            GetCurrentView(False)
            If sDataDetail = "" Then myReader.Close()
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

    End Sub
    'Διαγραφή όψης
    Private Sub popDeleteView_ItemClick(sender As Object, e As ItemClickEventArgs) Handles popDeleteView.ItemClick
        If XtraMessageBox.Show("Θέλετε να διαγραφεί η τρέχουσα όψη?", "PRIAMOS .NET", MessageBoxButtons.YesNo, MessageBoxIcon.Question) = vbYes Then
            If BarViews.EditValue <> "" Then
                My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue)
                If sDataDetail <> "" Then My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & BarViews.EditValue)
                CType(BarViews.Edit, RepositoryItemComboBox).Items.Remove(BarViews.EditValue)
                BarViews.EditValue = "" : CurrentView = "" : popSaveView.Enabled = False
            End If
        End If

    End Sub
    'Αποθήκευση ως όψης
    Private Sub RepositoryPopSaveAsView_KeyDown(sender As Object, e As KeyEventArgs) Handles RepositoryPopSaveAsView.KeyDown
        Try
            If e.KeyCode = Keys.Enter Then
                If GridView1.OptionsLayout.LayoutVersion <> "" Then
                    Dim sVer As Integer = GridView1.OptionsLayout.LayoutVersion.Replace("v", "")
                    GridView1.OptionsLayout.LayoutVersion = "v" & sVer + 1
                Else
                    GridView1.OptionsLayout.LayoutVersion = "v1"
                End If
                GridView1.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & sender.EditValue & "_" & UserProps.Code & ".xml", OptionsLayoutBase.FullLayout)
                If sDataDetail <> "" Then GridView2.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & sender.EditValue & "_" & UserProps.Code & ".xml", OptionsLayoutBase.FullLayout)
                'grdMain.DefaultView.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & sender.EditValue & "_" & UserProps.Code & ".xml")
                CType(BarViews.Edit, RepositoryItemComboBox).Items.Add(sender.EditValue & "_" & UserProps.Code & ".xml")

                BarViews.EditValue = sender.EditValue & "_" & UserProps.Code & ".xml"
                CurrentView = BarViews.EditValue
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
    'Αποθήκευση όψης
    Private Sub popSaveView_ItemClick(sender As Object, e As ItemClickEventArgs) Handles popSaveView.ItemClick
        If BarViews.EditValue <> "" Then
            'grdMain.DefaultView.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue)
            My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue)
            If GridView1.OptionsLayout.LayoutVersion <> "" Then
                Dim sVer As Integer = GridView1.OptionsLayout.LayoutVersion.Replace("v", "")
                GridView1.OptionsLayout.LayoutVersion = "v" & sVer + 1
            Else
                GridView1.OptionsLayout.LayoutVersion = "v1"
            End If
            GridView1.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
            If sDataDetail <> "" Then
                My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & BarViews.EditValue)
                GridView2.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
            End If
            'GridView1.SaveLayoutToXml(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & BarViews.EditValue)
            XtraMessageBox.Show("Η όψη αποθηκέυτηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub
    'Επαναφορά Default όψης
    Private Sub popRestoreView_ItemClick(sender As Object, e As ItemClickEventArgs) Handles popRestoreView.ItemClick
        grdMain.DefaultView.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml", OptionsLayoutBase.FullLayout)
        BarViews.EditValue = "" : popSaveAsView.EditValue = "" : popSaveView.Enabled = False : popDeleteView.Enabled = False
        CurrentView = ""
    End Sub

    Private Sub RepositoryBarViews_SelectedIndexChanged(sender As Object, e As EventArgs) Handles RepositoryBarViews.SelectedIndexChanged
        My.Settings.CurrentView = sender.EditValue
        My.Settings.Save()
    End Sub

    Private Sub RepositoryBarRecords_SelectedIndexChanged(sender As Object, e As EventArgs) Handles RepositoryBarRecords.SelectedIndexChanged
        My.Settings.Records = BarRecords.EditValue
        My.Settings.Save()
        LoadRecords()
    End Sub

    'Προσθήκη επιλογών στο Standar Header Menu
    Private Sub GridView1_PopupMenuShowing(sender As Object, e As PopupMenuShowingEventArgs) Handles GridView1.PopupMenuShowing
        If e.MenuType = GridMenuType.Column Then
            Dim menu As DevExpress.XtraGrid.Menu.GridViewColumnMenu = TryCast(e.Menu, GridViewColumnMenu)
            Dim item As New DXEditMenuItem()
            Dim itemColor As New DXEditMenuItem()

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

            End If
        Else
            PopupMenuRows.ShowPopup(System.Windows.Forms.Control.MousePosition)
        End If
    End Sub
    'Προσθήκη επιλογών στο Standar Detail Menu
    Private Sub GridView2_PopupMenuShowing(sender As Object, e As PopupMenuShowingEventArgs) Handles GridView2.PopupMenuShowing
        If e.MenuType = GridMenuType.Column Then
            Dim menu As DevExpress.XtraGrid.Menu.GridViewColumnMenu = TryCast(e.Menu, GridViewColumnMenu)
            Dim item As New DXEditMenuItem()
            Dim itemColor As New DXEditMenuItem()

            'menu.Items.Clear()
            If menu.Column IsNot Nothing Then
                'Για να προσθέσουμε menu item στο Default menu πρέπει πρώτα να προσθέσουμε ένα Repository Item 
                'Υπάρχουν πολλών ειδών Repositorys
                '1st Custom Menu Item
                Dim popRenameColumn As New RepositoryItemTextEdit
                popRenameColumn.Name = "RenameColumn"
                menu.Items.Add(New DXEditMenuItem("Μετονομασία Στήλης", popRenameColumn, AddressOf OnDetailEditValueChanged, Nothing, Nothing, 100, 0))
                item = menu.Items.Item("Μετονομασία Στήλης")
                item.EditValue = menu.Column.GetTextCaption
                item.Tag = menu.Column.AbsoluteIndex
                '2nd Custom Menu Item
                menu.Items.Add(CreateCheckItemDetail("Κλείδωμα Στήλης", menu.Column, Nothing))

                '3rd Custom Menu Item
                Dim popColorsColumn As New RepositoryItemColorEdit
                popColorsColumn.Name = "ColorsColumn"
                menu.Items.Add(New DXEditMenuItem("Χρώμα Στήλης", popColorsColumn, AddressOf OnDetailColumnsColorChanged, Nothing, Nothing, 100, 0))
                itemColor = menu.Items.Item("Χρώμα Στήλης")
                itemColor.EditValue = menu.Column.AppearanceCell.BackColor
                itemColor.Tag = menu.Column.AbsoluteIndex
            End If
        Else
            PopupMenuRowsDetail.ShowPopup(System.Windows.Forms.Control.MousePosition)
        End If
    End Sub
    'Αλλαγή Χρώματος Στήλης Master
    Private Sub OnColumnsColorChanged(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As DXEditMenuItem = TryCast(sender, DXEditMenuItem)
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView1.Columns(item.Tag).AppearanceCell.BackColor = item.EditValue
    End Sub
    'Αλλαγή Χρώματος Στήλης Detail
    Private Sub OnDetailColumnsColorChanged(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As DXEditMenuItem = TryCast(sender, DXEditMenuItem)
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView2.Columns(item.Tag).AppearanceCell.BackColor = item.EditValue
    End Sub
    'Μετονομασία Στήλης Master
    Private Sub OnEditValueChanged(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As New DXEditMenuItem()
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView1.Columns(item.Tag).Caption = item.EditValue
        'MessageBox.Show(item.EditValue.ToString())
    End Sub
    'Μετονομασία Στήλης Detail
    Private Sub OnDetailEditValueChanged(ByVal sender As System.Object, ByVal e As EventArgs)
        Dim item As New DXEditMenuItem()
        item = sender
        If item.Tag Is Nothing Then Exit Sub
        GridView2.Columns(item.Tag).Caption = item.EditValue
        'MessageBox.Show(item.EditValue.ToString())
    End Sub
    'Κλείδωμα Στήλης Master
    Private Sub OnCanMoveItemClick(ByVal sender As Object, ByVal e As EventArgs)
        Dim item As DXMenuCheckItem = TryCast(sender, DXMenuCheckItem)
        Dim info As MenuColumnInfo = TryCast(item.Tag, MenuColumnInfo)
        If info Is Nothing Then
            Return
        End If
        info.Column.OptionsColumn.AllowMove = Not item.Checked
    End Sub
    'Κλείδωμα Στήλης Detail
    Private Sub OnCanMoveItemClickDetail(ByVal sender As Object, ByVal e As EventArgs)
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
    Private Function CreateCheckItemDetail(ByVal caption As String, ByVal column As GridColumn, ByVal image As Image) As DXMenuCheckItem
        Dim item As New DXMenuCheckItem(caption, (Not column.OptionsColumn.AllowMove), image, New EventHandler(AddressOf OnCanMoveItemClickDetail))
        item.Tag = New MenuColumnInfo(column)
        Return item
    End Function
    'Print Preview
    Private Sub BarPrintPreview_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarPrintPreview.ItemClick
        GridView1.GridControl.ShowRibbonPrintPreview()
    End Sub
    'XLSX Export
    Private Sub BarExportXLSX_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarExportXLSX.ItemClick
        Dim options = New XlsxExportOptionsEx()
        options.UnboundExpressionExportMode = UnboundExpressionExportMode.AsFormula
        XtraSaveFileDialog1.Filter = "XLSX Files (*.xlsx*)|*.xlsx"
        If XtraSaveFileDialog1.ShowDialog() = DialogResult.OK Then
            GridView1.GridControl.ExportToXlsx(XtraSaveFileDialog1.FileName, options)
            Process.Start(XtraSaveFileDialog1.FileName)
        End If
    End Sub
    'PDF Export
    Private Sub BarPDFExport_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarPDFExport.ItemClick
        XtraSaveFileDialog1.Filter = "PDF Files (*.pdf*)|*.pdf"
        If XtraSaveFileDialog1.ShowDialog() = DialogResult.OK Then
            GridView1.GridControl.ExportToPdf(XtraSaveFileDialog1.FileName)
            Process.Start(XtraSaveFileDialog1.FileName)
        End If
    End Sub

    Friend Class MenuColumnInfo
        Public Sub New(ByVal column As GridColumn)
            Me.Column = column
        End Sub
        Public Column As GridColumn
    End Class
    ' Πάιρνω από την βάση την τρέχουσα όψη του χρήστη
    Private Sub GetCurrentView(ByVal GetVal As Boolean)
        Dim Cmd As SqlCommand, sdr As SqlDataReader
        Try
            If GetVal Then
                Cmd = New SqlCommand("SELECT currentview FROM USR_V WHERE USRID = '" & UserProps.ID.ToString & "' and  DATATABLE = '" & sDataTable & "'", CNDB)
                sdr = Cmd.ExecuteReader()
                If (sdr.Read() = True) Then
                    If sdr.IsDBNull(sdr.GetOrdinal("currentview")) = False Then CurrentView = sdr.GetString(sdr.GetOrdinal("currentview"))
                    'Έλεγχος αν το τελευταίο σχέδιο που έχει αποθηκευτεί στην βάση υπάρχει στον δίσκο
                    If My.Computer.FileSystem.FileExists(Application.StartupPath & "\DSGNS\" & sDataTable & "\" & CurrentView) = False Then CurrentView = ""
                Else
                    CurrentView = ""
                End If
                sdr.Close()

            Else
                If CurrentView <> "" Then
                    Cmd = CNDB.CreateCommand
                    Cmd.CommandType = CommandType.StoredProcedure
                    Cmd.Parameters.Add(New SqlParameter("@sDataTable", sDataTable))
                    Cmd.Parameters.Add(New SqlParameter("@ID", UserProps.ID))
                    Cmd.Parameters.Add(New SqlParameter("@CurrentView", CurrentView))
                    Cmd.CommandText = "SetUserView"
                    Cmd.ExecuteNonQuery()
                End If
            End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
            If GetVal Then sdr.Close()
        End Try

    End Sub
    Private Sub GridView1_DoubleClick(sender As Object, e As EventArgs) Handles GridView1.DoubleClick
        If GridView1.IsGroupRow(GridView1.FocusedRowHandle) Then Exit Sub Else EditRecord()
    End Sub
    'Νέα Εγγραφή
    Private Sub BarNewRec_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarNewRec.ItemClick
        NewRecord()
    End Sub
    'Επεξεργασία Εγγραφής
    Private Sub BarEdit_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarEdit.ItemClick
        EditRecord()
    End Sub
    'Διαγραφή Εγγραφής
    Private Sub BarDelete_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarDelete.ItemClick
        DeleteRecord()
    End Sub
    'Ανανέωση εγγραφών
    Private Sub BarRefresh_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarRefresh.ItemClick
        LoadRecords()
    End Sub
    'Επεξεργασία Εγγραφής
    Private Sub EditRecord()
        Dim fUsers As frmUsers = New frmUsers()
        Dim fMailSettings As frmMailSettings = New frmMailSettings()
        Dim fPermissions As frmPermissions = New frmPermissions()
        Dim fBDG As frmBDG = New frmBDG()
        Dim fCustomers As frmCustomers = New frmCustomers()
        Dim fTechicalSupport As frmTecnicalSupport = New frmTecnicalSupport()
        Dim fExp As frmEXP = New frmEXP()
        Dim fINH As frmINH = New frmINH()
        Dim fParameters As frmParameters = New frmParameters()
        Dim fGen As frmGen = New frmGen()

        Select Case sDataTable
            Case "vw_INH"
                fINH.Text = "Κοινόχρηστα"
                fINH.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fINH.MdiParent = frmMain
                fINH.Mode = FormMode.EditRecord
                fINH.Scroller = GridView1
                fINH.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fINH), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fINH.Show()
            Case "vw_EXP"
                fExp.Text = "Έξοδα"
                fExp.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fExp.MdiParent = frmMain
                fExp.Mode = FormMode.EditRecord
                fExp.Scroller = GridView1
                fExp.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fExp), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fExp.Show()
            Case "vw_TECH_SUP"
                fTechicalSupport.Text = "Διαχείριση Τεχνικής Υποστήριξης"
                fTechicalSupport.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fTechicalSupport.MdiParent = frmMain
                fTechicalSupport.Mode = FormMode.EditRecord
                fTechicalSupport.Scroller = GridView1
                fTechicalSupport.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fTechicalSupport), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fTechicalSupport.Show()
            Case "vw_USR"
                fUsers.Text = "Διαχείριση Χρηστών"
                fUsers.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fUsers.MdiParent = frmMain
                fUsers.Mode = FormMode.EditRecord
                fUsers.Scroller = GridView1
                fUsers.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fUsers), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fUsers.Show()
            Case "vw_MAILS"
                fMailSettings.Text = "Email Settings"
                fMailSettings.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fMailSettings.MdiParent = frmMain
                fMailSettings.Mode = FormMode.EditRecord
                fMailSettings.Scroller = GridView1
                fMailSettings.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fMailSettings), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fMailSettings.Show()
            Case "vw_RIGHTS"
                fPermissions.Text = "Δικαιώματα"
                fPermissions.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fPermissions.MdiParent = frmMain
                fPermissions.Mode = FormMode.EditRecord
                fPermissions.Scroller = GridView1
                fPermissions.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fPermissions), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fPermissions.Show()
            Case "vw_BDG"
                fBDG.Text = "Πολυκατοικίες"
                fBDG.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fBDG.bManageID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "bManageID").ToString
                fBDG.MdiParent = frmMain
                fBDG.Mode = FormMode.EditRecord
                fBDG.Scroller = GridView1
                fBDG.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fBDG), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fBDG.Show()
            Case "vw_CCT"
                fCustomers.Text = "Επαφές"
                fCustomers.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fCustomers.MdiParent = frmMain
                fCustomers.Mode = FormMode.EditRecord
                fCustomers.Scroller = GridView1
                fCustomers.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fCustomers), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fCustomers.Show()
            Case "vw_PRM"
                fParameters.Text = "Παράμετροι"
                fParameters.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fParameters.MdiParent = frmMain
                fParameters.Mode = FormMode.EditRecord
                fParameters.Scroller = GridView1
                fParameters.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fParameters), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fParameters.Show()
            Case "vw_AREAS"
                fGen.Text = "Περιοχές"
                fGen.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.EditRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "AREAS"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Περιοχή"
                fGen.L3.Text = "Νομός"
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_ADR"
                fGen.Text = "Διευθύνσεις"
                fGen.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.EditRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "ADR"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Διεύθυνση"
                fGen.L3.Text = "Νομός"
                fGen.L4.Text = "Περιοχές"
                fGen.L7.Text = "ΤΚ"
                fGen.L7.Control.Tag = "tk,0,1,2"
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                'fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_CALC_TYPES"
                fGen.Text = "Τύποι Υπολογισμού"
                fGen.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.EditRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "CALC_TYPES"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Όνομα"
                fGen.chk1.Text = "Ενεργό"
                fGen.L7.Text = "Τύπος"
                fGen.txtL7.Tag = "type,0,1,2"
                fGen.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_CALC_CAT"
                fGen.Text = "Κατηγορίες Υπολογισμών"
                fGen.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.EditRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "CALC_CAT"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Όνομα"
                fGen.L3.Text = "Τύπος Υπολογισμού"
                fGen.L3.Control.Tag = "calcTypeID,0,1,2"
                fGen.L3.Tag = ""
                fGen.L3.ImageOptions.Image = Nothing
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_MLC"
                fGen.Text = "Κατηγορίες Χιλιοστών"
                fGen.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.EditRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "MLC"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Κατηγορία"
                'fGen.L3.Text = "Τύπος Υπολογισμού"
                'fGen.cbo1.Tag = "calcID,0,1,2"
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                fGen.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                'fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()

            Case "vw_COU", "vw_DOY", "vw_PRF", "vw_HTYPES", "vw_BTYPES", "vw_FTYPES", "vw_TECH_CAT", "vw_CALC_CAT"
                Select Case sDataTable
                    Case "vw_COU" : fGen.Text = "Νομοί" : fGen.DataTable = "COU" : fGen.L2.Text = "Νομός"
                    Case "vw_DOY" : fGen.Text = "ΔΟΥ" : fGen.DataTable = "DOY" : fGen.L2.Text = "ΔΟΥ"
                    Case "vw_PRF" : fGen.Text = "Επαγγέλματα" : fGen.DataTable = "PRF" : fGen.L2.Text = "Επάγγελμα"
                    Case "vw_FTYPES" : fGen.Text = "Τύποι Καυσίμων" : fGen.DataTable = "FTYPES" : fGen.L2.Text = "Τύπος"
                    Case "vw_TECH_CAT" : fGen.Text = "Κατηγορίες Τεχνικής Υποστήριξης" : fGen.DataTable = "TECH_CAT" : fGen.L2.Text = "Κατηγορία"
                End Select
                fGen.ID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "ID").ToString
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.EditRecord
                fGen.Scroller = GridView1
                fGen.FormScroller = Me
                fGen.L1.Text = "Κωδικός"
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                fGen.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
        End Select
    End Sub
    'Νέα Εγγραφή
    Private Sub NewRecord()
        Dim fUsers As frmUsers = New frmUsers()
        Dim fMailSettings As frmMailSettings = New frmMailSettings()
        Dim fPermissions As frmPermissions = New frmPermissions()
        Dim fBDG As frmBDG = New frmBDG()
        Dim fCustomers As frmCustomers = New frmCustomers()
        Dim fParameters As frmParameters = New frmParameters()
        Dim fGen As frmGen = New frmGen()
        Dim fTechicalSupport As frmTecnicalSupport = New frmTecnicalSupport()
        Dim fINH As frmINH = New frmINH()

        Dim fExp As frmEXP = New frmEXP()



        Select Case sDataTable
            Case "vw_INH"
                fINH.Text = "Έξοδα"
                fINH.MdiParent = frmMain
                fINH.Mode = FormMode.NewRecord
                fINH.Scroller = GridView1
                fINH.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fINH), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fINH.Show()
            Case "vw_EXP"
                fExp.Text = "Έξοδα"
                fExp.MdiParent = frmMain
                fExp.Mode = FormMode.NewRecord
                fExp.Scroller = GridView1
                fExp.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fExp), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fExp.Show()
            Case "vw_TECH_SUP"
                fTechicalSupport.Text = "Διαχείριση Τεχνικής Υποστήριξης"
                fTechicalSupport.MdiParent = frmMain
                fTechicalSupport.Mode = FormMode.NewRecord
                fTechicalSupport.Scroller = GridView1
                fTechicalSupport.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fTechicalSupport), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fTechicalSupport.Show()
            Case "vw_USR"
                fUsers.Text = "Διαχείριση Χρηστών"
                fUsers.MdiParent = frmMain
                fUsers.Mode = FormMode.NewRecord
                fUsers.Scroller = GridView1
                fUsers.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fUsers), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fUsers.Show()
            Case "vw_MAILS"
                fMailSettings.Text = "Email Settings"
                fMailSettings.MdiParent = frmMain
                fMailSettings.Mode = FormMode.NewRecord
                fMailSettings.Scroller = GridView1
                fMailSettings.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fMailSettings), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fMailSettings.Show()
            Case "vw_RIGHTS"
                fPermissions.Text = "Δικαιώματα"
                fPermissions.MdiParent = frmMain
                fPermissions.Mode = FormMode.NewRecord
                fPermissions.Scroller = GridView1
                fPermissions.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fPermissions), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fPermissions.Show()
            Case "vw_BDG"
                fBDG.Text = "Πολυκατοικίες"
                fBDG.bManageID = GridView1.GetRowCellValue(GridView1.FocusedRowHandle, "bManageID").ToString
                fBDG.MdiParent = frmMain
                fBDG.Mode = FormMode.NewRecord
                fBDG.Scroller = GridView1
                fBDG.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fBDG), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fBDG.Show()
            Case "vw_CCT"
                fCustomers.Text = "Πελάτες"
                fCustomers.MdiParent = frmMain
                fCustomers.Mode = FormMode.NewRecord
                fCustomers.Scroller = GridView1
                fCustomers.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fCustomers), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fCustomers.Show()
            Case "vw_PRM"
                fParameters.Text = "Παράμετροι"
                fParameters.MdiParent = frmMain
                fParameters.Mode = FormMode.NewRecord
                fParameters.Scroller = GridView1
                fParameters.FormScroller = Me
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fParameters), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fParameters.Show()
            Case "vw_AREAS"
                fGen.Text = "Περιοχές"
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.NewRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "AREAS"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Περιοχή"
                fGen.L3.Text = "Νομός"
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_ADR"
                fGen.Text = "Διευθύνσεις"
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.NewRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "ADR"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Διεύθυνση"
                fGen.L3.Text = "Νομός"
                fGen.L4.Text = "Περιοχές"
                fGen.L7.Text = "ΤΚ"
                fGen.L7.Control.Tag = "tk,0,1,2"
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                'fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_CALC_TYPES"
                fGen.Text = "Τύποι Υπολογισμού"
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.NewRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "CALC_TYPES"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Όνομα"
                fGen.chk1.Text = "Ενεργό"
                fGen.L7.Text = "Τύπος"
                fGen.txtL7.Tag = "type,0,1,2"
                fGen.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_CALC_CAT"
                fGen.Text = "Κατηγορίες Υπολογισμών"
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.NewRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "CALC_CAT"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Όνομα"
                fGen.L3.Text = "Τύπος Υπολογισμού"
                fGen.L3.Control.Tag = "calcTypeID,0,1,2"
                fGen.L3.Tag = ""
                fGen.L3.ImageOptions.Image = Nothing
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_MLC"
                fGen.Text = "Κατηγορίες Χιλιοστών"
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.NewRecord
                fGen.Scroller = GridView1
                fGen.DataTable = "MLC"
                fGen.L1.Text = "Κωδικός"
                fGen.L2.Text = "Κατηγορία"
                fGen.L6.Text = "Χρώμα"
                'fGen.L3.Text = "Τύπος Υπολογισμού"
                'fGen.cbo1.Tag = "calcID,0,1,2"
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                fGen.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                'fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
            Case "vw_COU", "vw_DOY", "vw_PRF", "vw_HTYPES", "vw_BTYPES", "vw_FTYPES", "vw_TECH_CAT", "vw_CALC_CAT"
                Select Case sDataTable
                    Case "vw_COU" : fGen.Text = "Νομοί" : fGen.DataTable = "COU" : fGen.L2.Text = "Νομός"
                    Case "vw_DOY" : fGen.Text = "ΔΟΥ" : fGen.DataTable = "DOY" : fGen.L2.Text = "ΔΟΥ"
                    Case "vw_PRF" : fGen.Text = "Επαγγέλματα" : fGen.DataTable = "PRF" : fGen.L2.Text = "Επάγγελμα"
                    Case "vw_FTYPES" : fGen.Text = "Τύποι Καυσίμων" : fGen.DataTable = "FTYPES" : fGen.L2.Text = "Τύπος"
                    Case "vw_TECH_CAT" : fGen.Text = "Κατηγορίες Τεχνικής Υποστήριξης" : fGen.DataTable = "TECH_CAT" : fGen.L2.Text = "Κατηγορία"
                End Select
                fGen.MdiParent = frmMain
                fGen.Mode = FormMode.NewRecord
                fGen.Scroller = GridView1
                fGen.FormScroller = Me
                fGen.L1.Text = "Κωδικός"
                fGen.FormScroller = Me
                fGen.CalledFromControl = False
                fGen.L3.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L4.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L5.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L6.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                fGen.L7.Visibility = DevExpress.XtraLayout.Utils.LayoutVisibility.Never
                frmMain.XtraTabbedMdiManager1.Float(frmMain.XtraTabbedMdiManager1.Pages(fGen), New Point(CInt(Me.Parent.ClientRectangle.Width / 2 - Me.Width / 2), CInt(Me.Parent.ClientRectangle.Height / 2 - Me.Height / 2)))
                fGen.Show()
        End Select
    End Sub
    'Φορτώνω τις εγγραφές στο GRID
    Public Sub LoadRecords(Optional ByVal sDataTable2 As String = "", Optional sGuid As String = "")
        Dim sSQL As String
        Dim sSQL2 As String
        Try
            If BarRecords.EditValue <> "ALL" And BarRecords.EditValue <> "" Then
                sSQL = "SELECT top " & BarRecords.EditValue & " * FROM " & IIf(sDataTable = "", sDataTable2, sDataTable)
                If sDataDetail <> "" Then sSQL2 = "SELECT top " & BarRecords.EditValue & " * FROM " & sDataDetail
            Else
                sSQL = "SELECT  * FROM " & IIf(sDataTable = "", sDataTable2, sDataTable)
                If sDataDetail <> "" Then sSQL2 = "SELECT  * FROM " & sDataDetail
            End If

            If sDataDetail = "" Then
                myCmd = CNDB.CreateCommand
                myCmd.CommandText = sSQL
                GridView1.Columns.Clear()
                myReader = myCmd.ExecuteReader()
                grdMain.DataSource = myReader
            Else
                Select Case sDataDetail
                    Case "vw_FORM_RIGHTS"
                        Dim AdapterMaster As New SqlDataAdapter(sSQL, CNDB)
                        Dim AdapterDetail As New SqlDataAdapter(sSQL2, CNDB)
                        Dim sdataSet As New DataSet()
                        AdapterMaster.Fill(sdataSet, IIf(sDataTable = "", sDataTable2, sDataTable))
                        AdapterDetail.Fill(sdataSet, sDataDetail)
                        Dim keyColumn As DataColumn = sdataSet.Tables(IIf(sDataTable = "", sDataTable2, sDataTable)).Columns("ID")
                        Dim foreignKeyColumn As DataColumn = sdataSet.Tables(sDataDetail).Columns("RID")
                        sdataSet.Relations.Add("Φόρμες", keyColumn, foreignKeyColumn)
                        GridView1.Columns.Clear()
                        GridView2.Columns.Clear()
                        grdMain.DataSource = sdataSet.Tables(IIf(sDataTable = "", sDataTable2, sDataTable))
                        grdMain.ForceInitialize()
                End Select
            End If
            grdMain.DefaultView.PopulateColumns()
            'Εαν δεν έχει data το Dataset αναγκαστικά προσθέτω μόνος μου τις στήλες
            If sDataDetail = "" Then
                If myReader.HasRows = False Then
                    GridView1.Columns.Clear()
                    For i As Integer = 0 To myReader.FieldCount - 1
                        Dim C As New GridColumn
                        C.Name = "col" & myReader.GetName(i).ToString
                        C.Caption = myReader.GetName(i).ToString
                        C.Visible = True
                        C.FieldName = myReader.GetName(i).ToString
                        GridView1.Columns.Add(C)
                    Next i
                Else
                    LoadViews()
                End If
            Else
                LoadViews()
            End If
            If sDataTable = "" And sDataTable2 <> "" Then sDataTable = sDataTable2

            'Φορτώνει όλες τις ονομασίες των στηλών από τον SQL. Από το πεδίο Description
            'LoadForms.LoadColumnDescriptionNames(grdMain, GridView1, , sDataTable)
            'If sGuid.Length > 0 Then
            '    Dim colID As GridColumn = GridView1.Columns("ID")
            '    Dim rowHandle As Integer = -1
            '    rowHandle = GridView1.LocateByDisplayText(rowHandle + 1, colID, sGuid)
            '    grdMain.RefreshDataSource()
            '    GridView1.SelectRow(rowHandle)
            '    GridView1.FocusedRowHandle = rowHandle
            '    GridView1.SetFocusedRowCellValue(colID, sGuid)
            '    GridView1.MakeRowVisible(rowHandle)
            '    GridView1.TopRowIndex = GridView1.GetVisibleIndex(GridView1.FocusedRowHandle)
            'End If
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try

        'If grdMain.DefaultView.DataRowCount <> 0 Then myReader.Close() 'myReader.Close()
    End Sub

    Private Sub grdMain_KeyDown(sender As Object, e As KeyEventArgs) Handles grdMain.KeyDown
        Select Case e.KeyCode
            Case Keys.F2 : If UserProps.AllowInsert = True Then NewRecord()
            Case Keys.F3 : If UserProps.AllowEdit = True Then EditRecord()
            Case Keys.F5 : LoadRecords()
            Case Keys.Delete : If UserProps.AllowDelete = True Then DeleteRecord()
        End Select
    End Sub

    Private Sub frmScroller_Resize(sender As Object, e As EventArgs) Handles Me.Resize
        If Me.WindowState = FormWindowState.Maximized Then frmMain.XtraTabbedMdiManager1.Dock(Me, frmMain.XtraTabbedMdiManager1)
    End Sub

    Private Sub GridView1_CustomColumnDisplayText(sender As Object, e As CustomColumnDisplayTextEventArgs) Handles GridView1.CustomColumnDisplayText
        If e.Column.FieldName.Contains("pwd") Then e.DisplayText = StrDup(e.DisplayText.Length, "*")

    End Sub

    Private Sub GridView1_KeyDown(sender As Object, e As KeyEventArgs) Handles GridView1.KeyDown
        Dim view As GridView = CType(sender, GridView)
        If e.Control AndAlso e.KeyCode = Keys.C Then
            If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
                Clipboard.SetText(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString())
            End If
            e.Handled = True
        End If
    End Sub
    'Ορίζουμε το Detail View στο GridView2 που προσθέσαμε στο Design.  
    Private Sub grdMain_ViewRegistered(sender As Object, e As DevExpress.XtraGrid.ViewOperationEventArgs) Handles grdMain.ViewRegistered
        GridView2 = TryCast(e.View, GridView)
        GridView2.FocusRectStyle = DevExpress.XtraGrid.Views.Grid.DrawFocusRectStyle.RowFullFocus
        GridView2.OptionsBehavior.Editable = False
        GridView2.OptionsBehavior.ReadOnly = True
        GridView2.OptionsLayout.Columns.StoreAllOptions = True
        GridView2.OptionsLayout.Columns.StoreAppearance = True
        GridView2.OptionsLayout.StoreAllOptions = True
        GridView2.OptionsLayout.StoreAppearance = True
        GridView2.OptionsLayout.StoreFormatRules = True
        GridView2.OptionsPrint.PrintPreview = True
        GridView2.OptionsSelection.EnableAppearanceFocusedCell = False
        GridView2.OptionsView.EnableAppearanceEvenRow = True
        If CurrentView = "" Then
            If sDataDetail <> "" Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\DEF\" & sDataDetail & "_def.xml", OptionsLayoutBase.FullLayout)
        Else
            If sDataDetail <> "" Then GridView2.RestoreLayoutFromXml(Application.StartupPath & "\DSGNS\" & sDataDetail & "\" & BarViews.EditValue, OptionsLayoutBase.FullLayout)
        End If
    End Sub
    'Αποθήκευση όψης ως Default
    Private Sub popSaveAsDefault_ItemClick(sender As Object, e As ItemClickEventArgs) Handles popSaveAsDefault.ItemClick
        If GridView1.OptionsLayout.LayoutVersion <> "" Then
            Dim sVer As Integer = GridView1.OptionsLayout.LayoutVersion.Replace("v", "")
            GridView1.OptionsLayout.LayoutVersion = "v" & sVer + 1
        Else
            GridView1.OptionsLayout.LayoutVersion = "v1"
        End If
        GridView1.SaveLayoutToXml(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml", OptionsLayoutBase.FullLayout)
        If sDataDetail <> "" Then GridView2.SaveLayoutToXml(Application.StartupPath & "\DSGNS\DEF\" & sDataDetail & "_def.xml", OptionsLayoutBase.FullLayout)
        XtraMessageBox.Show("Η όψη αποθηκέυτηκε με επιτυχία", "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Information)

    End Sub
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
    'Copy Row
    Private Sub BarCopyRow_D_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarCopyRow_D.ItemClick
        Dim view As GridView = CType(GridView2, GridView)
        If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
            GridView2.OptionsSelection.MultiSelect = True
            GridView2.SelectRow(view.FocusedRowHandle)
            GridView2.CopyToClipboard()
            GridView2.OptionsSelection.MultiSelect = False
        End If
    End Sub
    'Copy All
    Private Sub BarCopyAll_D_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarCopyAll_D.ItemClick
        GridView2.OptionsSelection.MultiSelect = True
        GridView2.SelectAll()
        GridView2.CopyToClipboard()
        GridView2.OptionsSelection.MultiSelect = False
    End Sub
    ' Copy Cell
    Private Sub BarCopyCell_D_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarCopyCell_D.ItemClick
        Dim view As GridView = CType(GridView2, GridView)
        If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
            Clipboard.SetText(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString())
        End If
    End Sub

    Private Sub GridView2_KeyDown(sender As Object, e As KeyEventArgs) Handles GridView2.KeyDown
        Dim view As GridView = CType(sender, GridView)
        If e.Control AndAlso e.KeyCode = Keys.C Then
            If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
                Clipboard.SetText(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString())
            End If
            e.Handled = True
        End If
    End Sub

    Private Sub BBUpdateViewFromDB_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BBUpdateViewFromDB.ItemClick
        'ReadXml.UpdateXMLFile(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml")
        'My.Computer.FileSystem.DeleteFile(Application.StartupPath & "\DSGNS\DEF\" & sDataTable & "_def.xml")
        Dim col1 As GridColumn
        Dim grdColumns As List(Of GridColumn)
        LoadRecords()
        If myReader Is Nothing Then Exit Sub
        'Εαν υπάρχουν πεδία που πρέπει να προστεθούν από την βάση
        If myReader.FieldCount > GridView1.Columns.Count Then
            Dim schema As DataTable = myReader.GetSchemaTable()
            grdColumns = GridView1.Columns.ToList()
            For i As Integer = 0 To myReader.FieldCount - 1
                Console.WriteLine(myReader.GetName(i))
                Dim Col2 As GridColumn = GridView1.Columns(myReader.GetName(i))
                If Col2 Is Nothing Then
                    col1 = GridView1.Columns.AddField(myReader.GetName(i))
                    col1.FieldName = myReader.GetName(i)
                    col1.Visible = True
                    col1.VisibleIndex = 0
                    col1.AppearanceCell.BackColor = Color.Bisque
                End If

            Next
            'Εαν έχουν σβηστεί πεδία από την βάση τα αφαιρεί και από το grid
        ElseIf myReader.FieldCount < GridView1.Columns.Count Then
            Dim schema As DataTable = myReader.GetSchemaTable()
            grdColumns = GridView1.Columns.ToList()

            For i As Integer = 0 To grdColumns.Count - 1
                Try
                    Dim Col2 As GridColumn = grdColumns(i)
                    Dim sOrd As String = myReader.GetOrdinal(Col2.FieldName)
                Catch ex As Exception
                    Dim Col2 As GridColumn = grdColumns(i)
                    GridView1.Columns.Remove(Col2)
                    Console.WriteLine(ex.Message)

                    Continue For
                End Try

            Next

        End If


    End Sub
    ' Φίλτρο Με επιλογή
    Private Sub BarFilterWithCell_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarFilterWithCell.ItemClick
        Dim view As GridView = CType(GridView1, GridView)
        If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
            Dim filterString As String = "[" & GridView1.FocusedColumn.FieldName & "]" & "=" & toSQLValueS(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString())
            GridView1.Columns(GridView1.FocusedColumn.FieldName).FilterInfo = New ColumnFilterInfo(filterString)
        End If

    End Sub
    ' Αφαίρεση Φίλτρου
    Private Sub BarRemoveFilterWithCell_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarRemoveFilterWithCell.ItemClick
        GridView1.Columns(GridView1.FocusedColumn.FieldName).ClearFilter()
    End Sub
    ' Φίλτρο Με εξαίρεση
    Private Sub BarFilterWithoutCell_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarFilterWithoutCell.ItemClick
        Dim view As GridView = CType(GridView1, GridView)
        If view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn) IsNot Nothing AndAlso view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString() <> [String].Empty Then
            Dim filterString As String = "[" & GridView1.FocusedColumn.FieldName & "]" & "<>" & toSQLValueS(view.GetRowCellValue(view.FocusedRowHandle, view.FocusedColumn).ToString())
            GridView1.Columns(GridView1.FocusedColumn.FieldName).FilterInfo = New ColumnFilterInfo(filterString)
        End If

    End Sub
    'Αφαίρεση όλων των φίλτρων
    Private Sub BarRemoveAllFilters_ItemClick(sender As Object, e As ItemClickEventArgs) Handles BarRemoveAllFilters.ItemClick
        GridView1.ClearColumnsFilter()
    End Sub

    Private Sub GridView1_RowCellStyle(sender As Object, e As RowCellStyleEventArgs) Handles GridView1.RowCellStyle
        Try
            Select Case e.Column.FieldName
                Case "color" : If Not IsDBNull(e.CellValue) Then e.Appearance.BackColor = Color.FromArgb(e.CellValue)
            End Select
        Catch ex As Exception
            XtraMessageBox.Show(String.Format("Error: {0}", ex.Message), "PRIAMOS .NET", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub
End Class