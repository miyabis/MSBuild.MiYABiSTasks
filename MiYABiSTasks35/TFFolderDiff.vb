
Imports System.IO
Imports System.Text
Imports Microsoft.Win32

''' <summary>
''' 
''' </summary>
''' <remarks></remarks>
Public Class TFFolderDiff
	Inherits Task

#Region " Declare "

	Private Const _tfCmd As String = "folderdiff"
	Private Const _tfOptView As String = "/view:"
	Private Const _tfOptFilter As String = "/filter:"
	Private Const _tfOptFilterLocalPathsOnly As String = "/filterLocalPathsOnly"
	Private Const _tfOpt As String = "/recursive /noprompt"

	Private _installDir As String

	Private _vsversion As Integer

	Private _tfExePath As String
	Private _tfExeInfo As ProcessStartInfo

	Private _exitCode As Integer
	Private _standardOutput As StringBuilder
	Private _standardError As StringBuilder

	''' <summary>比較元フォルダ名</summary>
	Private _folderSrc As String
	''' <summary>比較先フォルダ名</summary>
	Private _folderDst As String

	''' <summary>比較元のフォルダにのみ存在するファイル名</summary>
	Private _srcOnlyFiles As List(Of ITaskItem)
	''' <summary>比較先のフォルダにのみ存在するファイル名</summary>
	Private _dstOnlyFiles As List(Of ITaskItem)
	''' <summary>比較元と比較先で異なるファイル名（比較元）</summary>
	Private _srcDifferentFiles As List(Of ITaskItem)
	''' <summary>比較元と比較先で異なるファイル名（比較先）</summary>
	Private _dstDifferentFiles As List(Of ITaskItem)

	Private _filter As String
	Private _filterLocalPathsOnly As Boolean

#End Region

#Region " コンストラクタ "

	''' <summary>
	''' デフォルトコンストラクタ
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		_init()
	End Sub

#End Region
#Region " プロパティ "

#Region " Input "

	''' <summary>
	''' 比較元のフォルダ名
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>必ず指定する</remarks>
	<Required()> _
	Public Property FolderSrc() As String
		Get
			Return _folderSrc
		End Get
		Set(ByVal value As String)
			_folderSrc = value
			'_tfExeInfo.WorkingDirectory = Path.GetFullPath(_folderSrc)
		End Set
	End Property

	''' <summary>
	''' 比較先のフォルダ名
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>必ず指定する</remarks>
	<Required()> _
	Public Property FolderDst() As String
		Get
			Return _folderDst
		End Get
		Set(ByVal value As String)
			_folderDst = value
		End Set
	End Property

	Public Property Filter As String
		Get
			Return _filter
		End Get
		Set(value As String)
			_filter = value
		End Set
	End Property

	Public Property FilterLocalPathsOnly As Boolean
		Get
			Return _filterLocalPathsOnly
		End Get
		Set(value As Boolean)
			_filterLocalPathsOnly = value
		End Set
	End Property

#End Region
#Region " Output "

	''' <summary>
	''' 比較元のフォルダにのみ存在するファイル名
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property SrcOnlyFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._srcOnlyFiles.ToArray
		End Get
	End Property

	''' <summary>
	''' 比較先のフォルダにのみ存在するファイル名
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property DstOnlyFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._dstOnlyFiles.ToArray
		End Get
	End Property

	''' <summary>
	''' 比較元と比較先で異なるファイル名（比較元）
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property SrcDifferentFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._srcDifferentFiles.ToArray
		End Get
	End Property

	''' <summary>
	''' 比較元と比較先で異なるファイル名（比較先）
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property DstDifferentFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._dstDifferentFiles.ToArray
		End Get
	End Property

#End Region

	Private ReadOnly Property IsExecute As Boolean
		Get
			Return Not String.IsNullOrEmpty(_tfExePath)
		End Get
	End Property

#End Region
#Region " Overrides Execute "

	''' <summary>
	''' タスクを実行します。
	''' </summary>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Overrides Function Execute() As Boolean

		_executeInit()

		If Not _executeSourceOnly() Then
			Return False
		End If
		If Not _executeTargetOnly() Then
			Return False
		End If
		If Not _executeSourceDifferent() Then
			Return False
		End If
		If Not _executeTargetDifferent() Then
			Return False
		End If

		Return Not Log.HasLoggedErrors
	End Function

#End Region
#Region " Methods "

	Private Function _executeInit() As Boolean
		_srcOnlyFiles = New List(Of ITaskItem)
		_dstOnlyFiles = New List(Of ITaskItem)
		_srcDifferentFiles = New List(Of ITaskItem)
		_dstDifferentFiles = New List(Of ITaskItem)
	End Function

	Private Function _executeSourceOnly() As Boolean
		Const cCMD As String = "sourceOnly"
		Dim fileList As String = String.Empty

		If Not _execute(cCMD, fileList) Then
			Return False
		End If

		_addList(fileList, _srcOnlyFiles)

		Return True
	End Function

	Private Function _executeTargetOnly() As Boolean
		Const cCMD As String = "targetOnly"
		Dim fileList As String = String.Empty

		If Not _execute(cCMD, fileList) Then
			Return False
		End If

		_addList(fileList, _dstOnlyFiles)

		Return True
	End Function

	Private Function _executeSourceDifferent() As Boolean
		Const cCMD As String = "different"
		Dim fileList As String = String.Empty

		If Not _execute(cCMD, fileList) Then
			Return False
		End If

		Dim r As New System.Text.RegularExpressions.Regex(" - \r\n\t.*\r\n")
		fileList = r.Replace(fileList, "")

		_addList(fileList, _srcDifferentFiles)

		Return True
	End Function

	Private Function _executeTargetDifferent() As Boolean
		Const cCMD As String = "different"
		Dim fileList As String = String.Empty

		If Not _execute(cCMD, fileList) Then
			Return False
		End If

		Dim r As New System.Text.RegularExpressions.Regex("\r\n.* - \r\n\t")
		fileList = r.Replace(fileList, "")

		_addList(fileList, _dstDifferentFiles)

		Return True
	End Function

	Private Function _execute(ByVal view As String, ByRef fileList As String) As Boolean
		Const cArguments As String = "{0} ""{1}"" ""{2}"" {3} {4}{5}"
		Dim args As String

		_standardOutput = New StringBuilder
		_standardError = New StringBuilder

		args = String.Format(cArguments, _tfCmd, Me.FolderSrc, Me.FolderDst, _tfOpt, _tfOptView, view)
		If Not String.IsNullOrEmpty(Me.Filter) Then
			args &= String.Format(" {0}""{1}""", _tfOptFilter, Me.Filter)
		End If
		If Me.FilterLocalPathsOnly Then
			args &= String.Format(" {0}", _tfOptFilterLocalPathsOnly)
		End If
		If Not _commandExecute(args) Then
			Return False
		End If

		Dim r As New System.Text.RegularExpressions.Regex("=.*\r\n.*\r\n.+=.*\r\n.*\r\n")
		fileList = r.Replace(_standardOutput.ToString, "")

		Return True
	End Function

	Private Function _commandExecute(ByVal args As String) As Boolean
		If Not Me.IsExecute Then
			Return False
		End If

		_tfExeInfo.Arguments = args

		_logMessage("Execute : {0} {1}", _tfExeInfo.FileName, _tfExeInfo.Arguments)

		Using tfExe As Process = New Process
			AddHandler tfExe.OutputDataReceived, AddressOf _outputHandler
			AddHandler tfExe.ErrorDataReceived, AddressOf _errorHandler

			tfExe.StartInfo = _tfExeInfo
			tfExe.Start()
			tfExe.BeginOutputReadLine()
			tfExe.BeginErrorReadLine()

			tfExe.WaitForExit()

			_exitCode = tfExe.ExitCode
		End Using

		If Not _exitCode.Equals(0) Then
			_logError(_standardError.ToString)
			Return False
		End If

		_logMessage(_standardOutput.ToString)
		Return True
	End Function

	Private Sub _outputHandler(ByVal o As Object, ByVal args As DataReceivedEventArgs)
		_standardOutput.AppendLine(args.Data) ' 出力されたデータを保存
	End Sub

	Private Sub _errorHandler(ByVal o As Object, ByVal args As DataReceivedEventArgs)
		_standardError.AppendLine(args.Data) ' 出力されたデータを保存
	End Sub

	Private Sub _init()
		Const C_TFEXE As String = "TF.exe"
		Const C_VALUE As String = "InstallDir"
		Dim vers() As Integer = {12, 11, 10}
		Dim key As RegistryKey = Nothing
		For Each ver As Integer In vers
			key = Registry.CurrentUser.OpenSubKey(String.Format("Software\Microsoft\VisualStudio\{0}.0_Config", ver))
			If key IsNot Nothing Then
				_vsversion = ver
				Exit For
			End If
		Next
		If key Is Nothing Then
			Return
		End If
		_installDir = CStr(key.GetValue(C_VALUE))
		_tfExePath = Path.Combine(Me._installDir, C_TFEXE)

		_tfExeInfo = New ProcessStartInfo()
		_tfExeInfo.LoadUserProfile = True
		_tfExeInfo.UseShellExecute = False
		_tfExeInfo.CreateNoWindow = True
		_tfExeInfo.RedirectStandardError = True
		_tfExeInfo.RedirectStandardOutput = True
		_tfExeInfo.FileName = _tfExePath

		_logMessage("TF.exe Path : {0}", _tfExePath)
	End Sub

	Private Sub _addList(ByVal fileList As String, ByVal lst As List(Of ITaskItem))
		Dim mc As System.Text.RegularExpressions.MatchCollection = System.Text.RegularExpressions.Regex.Matches(fileList, "(.*)\r\n")
		For Each m As System.Text.RegularExpressions.Match In mc
			If String.IsNullOrEmpty(m.Value) Then
				Continue For
			End If
			If m.Value.Equals(vbCrLf) Then
				Continue For
			End If
			Dim value As String = m.Value.Replace(vbCrLf, String.Empty)
			If Directory.Exists(value) Then
				Continue For
			End If
			lst.Add(New TaskItem(value))
		Next
	End Sub

	Private Sub _logMessage(ByVal message As String, ByVal ParamArray args() As Object)
		Try
			Log.LogMessage(message, args)
		Catch ex As Exception
			Debug.Print(message, args)
		End Try
	End Sub

	Private Sub _logError(ByVal message As String, ByVal ParamArray args() As Object)
		Try
			Log.LogError(message, args)
		Catch ex As Exception
			Debug.Print(message, args)
		End Try
	End Sub

#End Region

End Class
