
Imports System.IO

''' <summary>
''' 入力されたアイテムでアイテム コレクションを作成します。あるリストから別のリストにアイテムをコピーできます。
''' </summary>
''' <remarks>
''' <see cref="Microsoft.Build.Tasks.CreateItem"/> タスクの機能を拡張したタスクです。<br/>
''' <br/>
''' <list type="bullet">
''' <item><description>ファイルの属性を指定して読込める。</description></item>
''' </list>
''' <br/>
''' 出力パラメータは下記の通りです。<br/>
''' <br/>
''' <list type="table">
''' <listheader>
''' <term>出力パラメータ</term><description>説明</description>
''' </listheader>
''' <item><term>Include</term><description>対象となるファイルたち</description></item>
''' </list>
''' 
''' <example>
''' <code lang="xml">
''' <Project DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">
''' 	<Target Name="Build">
'''			<MiYABiS.Build.Tasks.CreateItem Include="$(MSBuildProjectDirectory)\test.txt" readonly="true">
'''				<Output TaskParameter="Include" ItemName="Files"/>
'''			</MiYABiS.Build.Tasks.CreateItem>
''' 		<Message Text="@(Files)"/>
''' 	</Target>
''' </Project>
''' </code>
''' </example>
''' 
''' </remarks>
Public Class CreateItemEx
	Inherits Task

	''' <summary>対象となるファイルたち</summary>
	Private _include As List(Of ITaskItem)

	''' <summary>対象外となるファイルたち</summary>
	Private _exclude As List(Of ITaskItem)

	Private _lines As List(Of ITaskItem)

#Region " コンストラクタ "

	''' <summary>
	''' デフォルトコンストラクタ
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		_include = New List(Of ITaskItem)
		_exclude = New List(Of ITaskItem)
		Me.ReadOnly = False
	End Sub

#End Region

#Region " プロパティ "

#Region " Input "

	''' <summary>
	''' 対象となるファイルたち
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>必ず指定する</remarks>
	<Required()> _
	Public Property Include() As ITaskItem()
		Get
			Return _lines.ToArray()
		End Get
		Set(ByVal value As ITaskItem())
			Dim createItemTask As CreateItem
			Dim items As List(Of ITaskItem)

			items = New List(Of ITaskItem)
			items.AddRange(value)

			createItemTask = New CreateItem()
			createItemTask.Include = items.ToArray
			If Not createItemTask.Execute() Then
				Throw New ArgumentException("source files set error.")
			End If

			Me._include.AddRange(createItemTask.Include)
		End Set
	End Property

	''' <summary>
	''' 対象外となるファイルたち
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property Exclude() As ITaskItem()
		Get
			Return _exclude.ToArray
		End Get
		Set(ByVal value As ITaskItem())
			Dim createItemTask As CreateItem
			Dim items As List(Of ITaskItem)

			items = New List(Of ITaskItem)
			items.AddRange(value)

			createItemTask = New CreateItem()
			createItemTask.Include = items.ToArray
			If Not createItemTask.Execute() Then
				Throw New ArgumentException("source files set error.")
			End If

			Me._exclude.AddRange(createItemTask.Include)
		End Set
	End Property

	''' <summary>
	''' 読み込み専用ファイルの状態
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks></remarks>
	Public Property [ReadOnly] As Boolean

#End Region

#End Region

#Region " Overrides Execute "

	Public Overrides Function Execute() As Boolean
		Dim createItemTask As CreateItem

		createItemTask = New CreateItem()
		createItemTask.Include = _include.ToArray
		createItemTask.Exclude = _exclude.ToArray
		If Not createItemTask.Execute() Then
			Throw New ArgumentException("source files set error.")
		End If

		_lines = New List(Of ITaskItem)

		For Each ff As ITaskItem In createItemTask.Include
			Dim attr As FileAttributes

			attr = File.GetAttributes(ff.ItemSpec)

			If Me.ReadOnly Then
				If ((attr And FileAttributes.ReadOnly) = FileAttributes.ReadOnly) Then
					_lines.Add(ff)
				End If
				Continue For
			Else
				If Not ((attr And FileAttributes.ReadOnly) = FileAttributes.ReadOnly) Then
					_lines.Add(ff)
				End If
				Continue For
			End If
		Next

		Return True
	End Function

#End Region

End Class
