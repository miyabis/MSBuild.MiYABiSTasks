
Imports System.IO

''' <summary>
''' �����R�[�h���w�肵�ăe�L�X�g �t�@�C������A�C�e���̈ꗗ��ǂݍ��݂܂��B
''' </summary>
''' <remarks>
''' <see cref="Microsoft.Build.Tasks.ReadLinesFromFile"/> �^�X�N�̋@�\���g�������^�X�N�ł��B<br/>
''' <br/>
''' <list type="bullet">
''' <item><description>�t�@�C���̕����R�[�h���w�肵�ēǍ��߂�B</description></item>
''' </list>
''' <br/>
''' �o�̓p�����[�^�͉��L�̒ʂ�ł��B<br/>
''' <br/>
''' <list type="table">
''' <listheader>
''' <term>�o�̓p�����[�^</term><description>����</description>
''' </listheader>
''' <item><term>Lines</term><description>�t�@�C������ǂݍ��񂾍s</description></item>
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

	''' <summary>�e�L�X�g�t�@�C����</summary>
	Private _file As String
	''' <summary>�e�L�X�g�t�@�C���̕����R�[�h</summary>
	Private _encoding As String
	''' <summary>�t�@�C������ǂݍ��񂾍s</summary>
	Private _lines As List(Of ITaskItem)

#Region " �R���X�g���N�^ "

	''' <summary>
	''' �f�t�H���g�R���X�g���N�^
	''' </summary>
	''' <remarks></remarks>
	Public Sub New()
		_encoding = "UTF-8"
	End Sub

#End Region
#Region " �v���p�e�B "

#Region " Input "

	''' <summary>
	''' �e�L�X�g�t�@�C����
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>�K���w�肷��</remarks>
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
	''' �e�L�X�g�t�@�C���̕����R�[�h
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ������Ƃ��́A�uUTF-8�v�Ƃ��܂��B
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
	''' �t�@�C������ǂݍ��񂾍s
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' ITaskItem �^�z��̏o�̓p�����[�^�ł��B
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
	''' �^�X�N�����s���܂��B
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
