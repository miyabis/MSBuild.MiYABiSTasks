
Imports System.IO

''' <summary>
''' ２つのフォルダ配下に存在するファイルを比較する。
''' </summary>
''' 
''' <remarks>
''' 比較するのは、ファイルの有無、ファイルの最終更新日付のみです。<br/>
''' ファイルの内容は比較しません。<br/>
''' <br/>
''' 出力パラメータは下記の通りです。<br/>
''' <br/>
''' <list type="table">
''' <listheader>
''' <term>出力パラメータ</term><description>説明</description>
''' </listheader>
''' <item><term>SrcOnly</term><description>比較元のみに存在するファイル</description></item>
''' <item><term>DstOnly</term><description>比較先のみに存在するファイル</description></item>
''' <item><term>SrcDifferent</term><description>双方に存在して更新日付が異なるファイル（比較元）</description></item>
''' <item><term>DstDifferent</term><description>双方に存在して更新日付が異なるファイル（比較先）</description></item>
''' </list>
''' 
''' <example>
''' <code lang="xml">
''' <Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
''' 	<Target Name="Build">
''' 		<MiYABiS.Build.Tasks.FolderDiff
''' 				FolderSrc="$(MSBuildProjectDirectory)\data"
''' 				FolderDst="$(MSBuildProjectDirectory)\コピー ～ data"
''' 				ExcludeSrc="$(MSBuildProjectDirectory)\data\**\*.scc"
''' 				ExcludeDst="$(MSBuildProjectDirectory)\コピー ～ data\**\*.log"
''' 				IgnoreCase="True">
''' 			<Output TaskParameter="SrcOnly" ItemName="SrcOnlyFiles"/>
''' 			<Output TaskParameter="DstOnly" ItemName="DstOnlyFiles"/>
''' 			<Output TaskParameter="SrcDifferent" ItemName="SrcDifferentFiles"/>
''' 			<Output TaskParameter="DstDifferent" ItemName="DstDifferentFiles"/>
''' 		</MiYABiS.Build.Tasks.FolderDiff>
''' 	</Target>
''' </Project>
''' </code>
''' </example>
''' 
''' </remarks>
Public Class FolderDiff
	Inherits Task

	''' <summary>比較元フォルダ名</summary>
	Private _folderSrc As String
	''' <summary>比較先フォルダ名</summary>
	Private _folderDst As String
	''' <summary>比較対象外のファイル</summary>
	Private _excludeSrc As List(Of ITaskItem)
	Private _excludeSrcNames As List(Of String)
	Private _excludeDst As List(Of ITaskItem)
	Private _excludeDstNames As List(Of String)

	''' <summary>大文字、小文字の区別</summary>
	Private _ignorecase As Boolean

	Private _srcOnly As List(Of ITaskItem)
	Private _dstOnly As List(Of ITaskItem)
	Private _srcDifferent As List(Of ITaskItem)
	Private _dstDifferent As List(Of ITaskItem)

#Region " コンストラクタ "

	''' <summary>
	''' デフォルトコンストラクタ
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		_excludeSrc = New List(Of ITaskItem)
		_excludeSrcNames = New List(Of String)
		_excludeDst = New List(Of ITaskItem)
		_excludeDstNames = New List(Of String)
		_ignorecase = True
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

	''' <summary>
	''' 大文字、小文字の区別
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能。True のときは、大文字と小文字の区別をします。
	''' False のときは、区別をしません。省略時は区別をします。
	''' </remarks>
	Public Property IgnoreCase() As System.Boolean
		Get
			Return Me._ignorecase
		End Get
		Set(ByVal value As System.Boolean)
			Me._ignorecase = value
		End Set
	End Property

	''' <summary>
	''' 比較元の比較対象外のファイル
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property ExcludeSrc() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._excludeSrc.ToArray()
		End Get
		Set(ByVal value As Microsoft.Build.Framework.ITaskItem())
			Dim createItemTask As CreateItemEx
			Dim items As List(Of ITaskItem)

			items = New List(Of ITaskItem)
			items.AddRange(value)

			createItemTask = New CreateItemEx()
			createItemTask.Include = items.ToArray
			If Not createItemTask.Execute() Then
				Throw New ArgumentException("Exclude files set error.")
			End If

			Me._excludeSrc.AddRange(createItemTask.Include)

			For Each item As ITaskItem In Me._excludeSrc
				_excludeSrcNames.Add(item.GetMetadata("FullPath"))
			Next
		End Set
	End Property

	''' <summary>
	''' 比較先の比較対象外のファイル
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property ExcludeDst() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._excludeDst.ToArray()
		End Get
		Set(ByVal value As Microsoft.Build.Framework.ITaskItem())
			Dim createItemTask As CreateItemEx
			Dim items As List(Of ITaskItem)

			items = New List(Of ITaskItem)
			items.AddRange(value)

			createItemTask = New CreateItemEx()
			createItemTask.Include = items.ToArray
			If Not createItemTask.Execute() Then
				Throw New ArgumentException("Exclude files set error.")
			End If

			Me._excludeDst.AddRange(createItemTask.Include)

			For Each item As ITaskItem In Me._excludeDst
				_excludeDstNames.Add(item.GetMetadata("FullPath"))
			Next
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
	Public ReadOnly Property SrcOnly() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._srcOnly.ToArray
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
	Public ReadOnly Property DstOnly() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._dstOnly.ToArray
		End Get
	End Property

	''' <summary>
	''' 比較元と比較先で最終更新日付が異なるファイル名（比較元）
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property SrcDifferent() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._srcDifferent.ToArray
		End Get
	End Property

	''' <summary>
	''' 比較元と比較先で最終更新日付が異なるファイル名（比較先）
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property DstDifferent() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._dstDifferent.ToArray
		End Get
	End Property

#End Region

#End Region
#Region " Overrides Execute "

	''' <summary>
	''' タスクを実行します。
	''' </summary>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Overrides Function Execute() As Boolean
		Dim lstSrc As SortedList
		Dim lstDst As SortedList

		lstSrc = _getChild(_folderSrc)
		lstDst = _getChild(_folderDst)
		'_log(lstSrc)
		'_log(lstDst)

		_srcOnly = New List(Of ITaskItem)
		_dstOnly = New List(Of ITaskItem)
		_srcDifferent = New List(Of ITaskItem)
		_dstDifferent = New List(Of ITaskItem)

		_diff(lstSrc, lstDst)

		Return Not Log.HasLoggedErrors
	End Function

	Private Sub _diff(ByVal src As SortedList, ByVal dst As SortedList)
		Dim dstBuf As SortedList

		If dst Is Nothing Then
			dst = New SortedList
		End If
		dstBuf = DirectCast(dst.Clone(), SortedList)

		For Each keySrc As String In src.Keys
			Dim keyDst As String
			Dim valSrc As Object
			Dim valDst As Object

			keyDst = keySrc.Replace(_folderSrc, _folderDst)
			valSrc = src.Item(keySrc)
			valDst = dst.Item(keyDst)

			dstBuf.Remove(keyDst)

			If TypeOf valSrc Is SortedList Then
				_diff(DirectCast(valSrc, SortedList), DirectCast(valDst, SortedList))
				Continue For
			End If

			' 比較対象外ファイルか？
			If _execludeFilesContains(_excludeSrcNames, keySrc) Then
				Continue For
			End If

			' 比較先に存在するか？
			If Not dst.GetKeyList.Contains(keyDst) Then
				_srcOnly.Add(New TaskItem(keySrc))
				Continue For
			End If

			' 同じか？
			If valSrc.Equals(valDst) Then
				Continue For
			End If

			' 日付が異なる
			_srcDifferent.Add(New TaskItem(keySrc))
			_dstDifferent.Add(New TaskItem(keyDst))
		Next

		_makeDstOnlyList(dstBuf)
	End Sub

	Private Sub _makeDstOnlyList(ByVal dstBuf As SortedList)
		For Each key As String In dstBuf.Keys
			' 比較対象外ファイルか？
			If _execludeFilesContains(_excludeDstNames, key) Then
				Continue For
			End If

			Dim value As Object
			value = dstBuf.Item(key)
			If TypeOf value Is SortedList Then
				_makeDstOnlyList(DirectCast(value, SortedList))
				Continue For
			End If
			Dim item As ITaskItem
			item = New TaskItem(key)
			_dstOnly.Add(item)
		Next
	End Sub

	Private Function _execludeFilesContains(ByVal src As List(Of String), ByVal dst As String) As Boolean
		If Not Me.IgnoreCase Then
			dst = dst.ToLower
		End If
		Return src.Contains(Path.GetFullPath(dst))
	End Function

	Private Function _getChild(ByVal folderName As String) As SortedList
		Dim lst As SortedList

		lst = New SortedList

		For Each dir As String In Directory.GetDirectories(folderName)
			lst.Add(dir, _getChild(dir))
		Next

		For Each nextFile As String In Directory.GetFiles(folderName)
			If Not ((File.GetAttributes(nextFile) And FileAttributes.System) = FileAttributes.System) And _
			  Not ((File.GetAttributes(nextFile) And FileAttributes.Hidden) = FileAttributes.Hidden) Then
			End If

			lst.Add(nextFile, File.GetLastWriteTime(nextFile))
		Next

		Return lst
	End Function

	Private Sub _log(ByVal lst As SortedList)
		For Each key As String In lst.Keys
			Dim obj As Object
			obj = lst.Item(key)
			If TypeOf obj Is SortedList Then
				_log(DirectCast(obj, SortedList))
				Continue For
			End If
			Debug.WriteLine(key & " = " & obj.ToString)
		Next
	End Sub

#End Region

End Class
