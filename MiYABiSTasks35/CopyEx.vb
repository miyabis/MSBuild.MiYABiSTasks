Imports System.IO

''' <summary>
''' ファイルやフォルダのコピーをする。
''' </summary>
''' 
''' <remarks>
''' MSBuild 標準の <see cref="Microsoft.Build.Tasks.Copy"/> タスクの機能を拡張したタスクです。<br/>
''' <br/>
''' <list type="bullet">
''' <item><description><c>DestinationFolder</c> が指定されたとき、コピー元のファイル名からパス名を取り除いた配下のパス構成をそのままコピーできます。</description></item>
''' <item><description><c>SkipUnchanged</c> が <c>True</c> のとき、コピーされなかったファイルが出力パラメータに含まれない。</description></item>
''' </list>
''' <br/>
''' 出力パラメータは下記の通りです。<br/>
''' <br/>
''' <list type="table">
''' <listheader>
''' <term>出力パラメータ</term><description>説明</description>
''' </listheader>
''' <item><term>CopiedFiles</term><description>実際にコピーしたファイル</description></item>
''' </list>
''' 
''' <example>
''' <code lang="xml">
''' <Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
''' 	<Target Name="Build">
'''			<CreateItem Include="$(MSBuildProjectDirectory)\data\**\*.*">
''' 			<Output TaskParameter="Include" ItemName="CopyFiles" />
''' 		</CreateItem>
''' 		<MiYABiS.Build.Tasks.Copy
''' 				  SourceFiles="@(CopyFiles)"
''' 				  DestinationFolder="C:\Temp\hoge"
''' 				  Overwrite="True"
''' 				  OverwriteReadOnly="True"
''' 				  SkipUnchanged="false"
''' 				  RemoveRoot="$(MSBuildProjectDirectory)\data"
''' 				  >
''' 			<Output TaskParameter="CopiedFiles" ItemName="Files"/>
''' 		</MiYABiS.Build.Tasks.Copy>
''' 		<Message Text="@(Files)"/>
''' 	</Target>
''' </Project>
''' </code>
''' </example>
''' 
''' </remarks>
Public Class CopyEx
	Inherits Task

	Private _sourceFiles As List(Of ITaskItem)
	Private _destinationFiles As List(Of ITaskItem)
	Private _copiedFiles As List(Of ITaskItem)
	Private _destinationFolder As String
	Private _removeRoot As String
	Private _overwrite As Boolean
	Private _overwriteReadOnly As Boolean
	Private _skipUnchanged As Boolean

#Region " コンストラクタ "

	''' <summary>
	''' デフォルトコンストラクタ
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		_sourceFiles = New List(Of ITaskItem)
		_destinationFiles = New List(Of ITaskItem)
		_copiedFiles = New List(Of ITaskItem)
		_overwrite = False
		_overwriteReadOnly = False
		_skipUnchanged = False
	End Sub

#End Region
#Region " プロパティ "

#Region " Input "

	''' <summary>
	''' コピー元となるファイル
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 必ず指定する ITaskItem 型配列のパラメータです。<br/>
	''' コピー元となるファイルを指定します。
	''' </remarks>
	<Required()> _
	Public Property SourceFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._sourceFiles.ToArray()
		End Get
		Set(ByVal value As Microsoft.Build.Framework.ITaskItem())
            Dim createItemTask As CreateItemEx
            Dim items As List(Of ITaskItem)

            items = New List(Of ITaskItem)
			items.AddRange(value)

            createItemTask = New CreateItemEx()
            createItemTask.Include = items.ToArray
            If Not createItemTask.Execute() Then
				Throw New ArgumentException("source files set error.")
			End If

			Me._sourceFiles.AddRange(createItemTask.Include)
		End Set
	End Property

	''' <summary>
	''' コピー先となるファイル
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な ITaskItem 型配列のパラメータです。<br/>
	''' ソース ファイルのコピー先ファイルの一覧を指定します。
	''' この一覧のファイルは、<see cref="SourceFiles"/> パラメータに指定した一覧の内容と 1 対 1 で対応している必要があります。
	''' つまり、<see cref="SourceFiles"/> の最初のファイルは、<c>DestinationFiles</c> の最初の場所にコピーされ、2 番目以降のファイルも同様に処理されます。
	''' </remarks>
	Public Property DestinationFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			If Me._destinationFiles.Count = 0 And Me._destinationFolder IsNot Nothing Then
				Dim items As List(Of ITaskItem)

				items = New List(Of ITaskItem)

				For Each src As ITaskItem In _sourceFiles
					Dim dst As String

					If _removeRoot IsNot Nothing Then
						dst = src.GetMetadata("FullPath").Replace(_removeRoot, _destinationFolder)
					Else
						dst = _destinationFolder
						dst &= Path.DirectorySeparatorChar
						dst &= src.GetMetadata("Filename")
						dst &= src.GetMetadata("Extension")
					End If
					items.Add(New TaskItem(dst))
				Next
				Me._destinationFiles.AddRange(items)
			End If
			Return Me._destinationFiles.ToArray
		End Get
		Set(ByVal value As Microsoft.Build.Framework.ITaskItem())
            Dim createItemTask As CreateItemEx
            Dim items As List(Of ITaskItem)

            items = New List(Of ITaskItem)
			items.AddRange(value)

            createItemTask = New CreateItemEx()
            createItemTask.Include = items.ToArray
            If Not createItemTask.Execute() Then
				Throw New ArgumentException("destination files set error.")
			End If

			Me._destinationFiles.AddRange(createItemTask.Include)
		End Set
	End Property

	''' <summary>
	''' ファイルのコピー先ディレクトリ
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な String 型のパラメータです。<br/>
	''' ファイルのコピー先ディレクトリを指定します。
	''' ファイルではなく、ディレクトリである必要があります。
	''' ディレクトリが存在しない場合は、自動的に作成され、直下へ全てのファイルがコピーされます。<br/>
	''' 直下へ全てのファイルをコピーしたくないときは、<see cref="DestinationFiles"/> で１対１で指定するか、<see cref="RemoveRoot"/> を指定してください。<br/>
	''' </remarks>
	Public Property DestinationFolder() As String
		Get
			Return Me._destinationFolder
		End Get
		Set(ByVal value As String)
			' 最後の「￥」は消す
			If value.EndsWith(Path.DirectorySeparatorChar) Then
				value = value.Substring(0, value.Length - 1)
			End If
			Me._destinationFolder = value
		End Set
	End Property

	''' <summary>
	''' コピー元のファイル名から取り除くパス名
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な String 型のパラメータです。<br/>
	''' コピー元のファイル名から取り除くパス名を指定します。<br/>
	''' 指定されたときは、一致したパス名より下のフォルダを維持したままコピーされます。<br/>
	''' 省略されたときは、<see cref="DestinationFiles"/> 又は、<see cref="DestinationFolder" />で指定された
	''' </remarks>
	Public Property RemoveRoot() As String
		Get
			Return Me._removeRoot
		End Get
		Set(ByVal value As String)
			If value.EndsWith(Path.DirectorySeparatorChar) Then
				value = value.Substring(0, value.Length - 1)
			End If
			Me._removeRoot = value
		End Set
	End Property

	''' <summary>
	''' ファイルの上書き扱い
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な Boolean 型のパラメータです。<br/>
	''' コピー先フォルダにファイルが既に存在していたとき、上書きするか指定します。<br/>
	''' <c>True</c> を指定すると上書きします。省略時は <c>False</c> となり上書きしません。<br/>
	''' </remarks>
	Public Property Overwrite() As Boolean
		Get
			Return Me._overwrite
		End Get
		Set(ByVal value As Boolean)
			Me._overwrite = value
		End Set
	End Property

	''' <summary>
	''' ファイルが読み取り専用だったときの扱い
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な Boolean 型のパラメータです。<br/>
	''' ファイルが読み取り専用としてマークされているとき、上書きするか指定します。<br/>
	''' <c>True</c> を指定すると上書きします。省略時は <c>False</c> となり上書きしません。<br/>
	''' </remarks>
	Public Property OverwriteReadOnly() As Boolean
		Get
			Return Me._overwriteReadOnly
		End Get
		Set(ByVal value As Boolean)
			Me._overwriteReadOnly = value
		End Set
	End Property

	''' <summary>
	''' 
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な Boolean 型のパラメータです。<br/>
	''' <c>True</c> に設定すると、コピー元のファイルとコピー先のファイルで変更がない場合、コピー処理がスキップされます。<br/>
	''' <see cref="Copy"/> タスクでは、ファイルのサイズが等しく、最終更新時刻が等しい場合、ファイルは変更されていないと見なされます。<br/>
	''' 省略時は <c>False</c> となりスキップしません。<br/>
	''' </remarks>
	Public Property SkipUnchanged() As Boolean
		Get
			Return Me._skipUnchanged
		End Get
		Set(ByVal value As Boolean)
			Me._skipUnchanged = value
		End Set
	End Property

#End Region
#Region " Output "

	''' <summary>
	''' 実際にコピーしたファイル
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略可能な ITaskItem 型配列の出力パラメータです。<br/>
	''' 正常にコピーされたアイテムが格納されます。<br/>
	''' <see cref="SkipUnchanged"/> が <c>True</c> のときにコピーされれなかったファイルは除きます。<br/>
	''' </remarks>
	<Output()> _
	Public ReadOnly Property CopiedFiles() As Microsoft.Build.Framework.ITaskItem()
		Get
			Return Me._copiedFiles.ToArray
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
		Try
			If _destinationFiles.Count = 0 And _destinationFolder Is Nothing Then
				Throw New Exception("You must specify only one attribute from DestinationFiles and DestinationFolder")
			End If
			If SourceFiles.Length <> DestinationFiles.Length Then
				Throw New Exception("Number of source files is different than number of destination files.")
			End If

			Dim src, dst As IEnumerator(Of ITaskItem)

			src = DirectCast(_sourceFiles.GetEnumerator(), IEnumerator(Of ITaskItem))
			dst = DirectCast(_destinationFiles.GetEnumerator(), IEnumerator(Of ITaskItem))

			While src.MoveNext()
				Dim srcFile, dstFile As String
				Dim chkDir As String

				dst.MoveNext()

				srcFile = src.Current.GetMetadata("FullPath")
				dstFile = dst.Current.GetMetadata("FullPath")

#If DEBUG Then
				Debug.WriteLine(String.Format("Copying it from ""{0}"" to ""{1}""", srcFile, dstFile))
#Else
                Log.LogMessage(MessageImportance.Normal, "Copying it from ""{0}"" to ""{1}""", srcFile, dstFile)
#End If

				' ファイルが存在するか？
				If File.Exists(dstFile) Then
					' 読み取り専用ファイル対処
					If _overwriteReadOnly Then
						If (File.GetAttributes(dstFile) And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then
							File.SetAttributes(dstFile, FileAttributes.Normal)
						End If
					End If
					' 最終更新時刻が異なるファイル対処
					If _skipUnchanged Then
						If File.GetLastWriteTime(srcFile) = File.GetLastWriteTime(dstFile) Then
							Continue While
						End If
					End If
				End If

				' コピー先のフォルダチェックして無ければ作成
				chkDir = Path.GetDirectoryName(dstFile)
				If Not Directory.Exists(chkDir) Then
					Directory.CreateDirectory(chkDir)
				End If

				' ファイルコピー
				File.Copy(srcFile, dstFile, _overwrite)
				_copiedFiles.Add(src.Current)
			End While

		Catch ex As Exception
#If DEBUG Then
			Debug.WriteLine(ex.ToString)
#Else
            Log.LogErrorFromException(ex)
#End If
		End Try

		Return Not Log.HasLoggedErrors
	End Function

#End Region

End Class
