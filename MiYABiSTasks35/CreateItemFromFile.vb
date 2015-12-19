
Imports System.IO

''' <summary>
''' 文字コードを指定してテキスト ファイルからアイテムの一覧を読み込みます。
''' </summary>
''' <remarks>
''' <see cref="Microsoft.Build.Tasks.ReadLinesFromFile"/> タスクの機能を拡張したタスクです。<br/>
''' <br/>
''' <list type="bullet">
''' <item><description>ファイルの文字コードを指定して読込める。</description></item>
''' </list>
''' <br/>
''' 出力パラメータは下記の通りです。<br/>
''' <br/>
''' <list type="table">
''' <listheader>
''' <term>出力パラメータ</term><description>説明</description>
''' </listheader>
''' <item><term>Lines</term><description>ファイルから読み込んだ行</description></item>
''' </list>
''' 
''' <example>
''' <code lang="xml">
''' <Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
''' 	<Target Name="Build">
'''			<MiYABiS.Build.Tasks.CreateItemFromFile File="$(MSBuildProjectDirectory)\test.txt" Encoding="shift-jis">
'''				<Output TaskParameter="Lines" ItemName="Files"/>
'''			</MiYABiS.Build.Tasks.CreateItemFromFile>
''' 		<Message Text="@(Files)"/>
''' 	</Target>
''' </Project>
''' </code>
''' </example>
''' 
''' </remarks>
Public Class CreateItemFromFile
	Inherits Task

	''' <summary>テキストファイル名</summary>
	Private _file As String
	''' <summary>テキストファイルの文字コード</summary>
	Private _encoding As String
	''' <summary>ファイルから読み込んだ行</summary>
	Private _lines As List(Of ITaskItem)

#Region " コンストラクタ "

	''' <summary>
	''' デフォルトコンストラクタ
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		_encoding = "UTF-8"
	End Sub

#End Region
#Region " プロパティ "

#Region " Input "

	''' <summary>
	''' テキストファイル名
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>必ず指定する</remarks>
	<Required()> _
	Public Property File() As String
		Get
			Return _file
		End Get
		Set(ByVal value As String)
			_file = value
		End Set
	End Property

	''' <summary>
	''' テキストファイルの文字コード
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' 省略したときは、「UTF-8」とします。
	''' </remarks>
	Public Property Encoding() As String
		Get
			Return _encoding
		End Get
		Set(ByVal value As String)
			_encoding = value
		End Set
	End Property

#End Region
#Region " Output "

	''' <summary>
	''' ファイルから読み込んだ行
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem 型配列の出力パラメータです。
	''' </remarks>
	<Output()> _
	Public ReadOnly Property Lines() As ITaskItem()
		Get
			Return _lines.ToArray
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
			_lines = New List(Of ITaskItem)

			Using sr As StreamReader = New StreamReader(_file, System.Text.Encoding.GetEncoding(_encoding))
				Dim line As String
				Dim item As ITaskItem

				Do
					line = sr.ReadLine()

					If line IsNot Nothing Then
						item = New TaskItem(line)
						_lines.Add(item)
					End If
				Loop Until line Is Nothing
				sr.Close()

			End Using
		Catch E As Exception
			Log.LogErrorFromException(E)
		End Try

		Return Not Log.HasLoggedErrors
	End Function

#End Region

End Class
