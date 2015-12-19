Imports System.IO

''' <summary>
''' �t�@�C����t�H���_�̃R�s�[������B
''' </summary>
''' 
''' <remarks>
''' MSBuild �W���� <see cref="Microsoft.Build.Tasks.Copy"/> �^�X�N�̋@�\���g�������^�X�N�ł��B<br/>
''' <br/>
''' <list type="bullet">
''' <item><description><c>DestinationFolder</c> ���w�肳�ꂽ�Ƃ��A�R�s�[���̃t�@�C��������p�X������菜�����z���̃p�X�\�������̂܂܃R�s�[�ł��܂��B</description></item>
''' <item><description><c>SkipUnchanged</c> �� <c>True</c> �̂Ƃ��A�R�s�[����Ȃ������t�@�C�����o�̓p�����[�^�Ɋ܂܂�Ȃ��B</description></item>
''' </list>
''' <br/>
''' �o�̓p�����[�^�͉��L�̒ʂ�ł��B<br/>
''' <br/>
''' <list type="table">
''' <listheader>
''' <term>�o�̓p�����[�^</term><description>����</description>
''' </listheader>
''' <item><term>CopiedFiles</term><description>���ۂɃR�s�[�����t�@�C��</description></item>
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

#Region " �R���X�g���N�^ "

	''' <summary>
	''' �f�t�H���g�R���X�g���N�^
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
#Region " �v���p�e�B "

#Region " Input "

	''' <summary>
	''' �R�s�[���ƂȂ�t�@�C��
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �K���w�肷�� ITaskItem �^�z��̃p�����[�^�ł��B<br/>
	''' �R�s�[���ƂȂ�t�@�C�����w�肵�܂��B
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
	''' �R�s�[��ƂȂ�t�@�C��
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ��\�� ITaskItem �^�z��̃p�����[�^�ł��B<br/>
	''' �\�[�X �t�@�C���̃R�s�[��t�@�C���̈ꗗ���w�肵�܂��B
	''' ���̈ꗗ�̃t�@�C���́A<see cref="SourceFiles"/> �p�����[�^�Ɏw�肵���ꗗ�̓��e�� 1 �� 1 �őΉ����Ă���K�v������܂��B
	''' �܂�A<see cref="SourceFiles"/> �̍ŏ��̃t�@�C���́A<c>DestinationFiles</c> �̍ŏ��̏ꏊ�ɃR�s�[����A2 �Ԗڈȍ~�̃t�@�C�������l�ɏ�������܂��B
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
	''' �t�@�C���̃R�s�[��f�B���N�g��
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ��\�� String �^�̃p�����[�^�ł��B<br/>
	''' �t�@�C���̃R�s�[��f�B���N�g�����w�肵�܂��B
	''' �t�@�C���ł͂Ȃ��A�f�B���N�g���ł���K�v������܂��B
	''' �f�B���N�g�������݂��Ȃ��ꍇ�́A�����I�ɍ쐬����A�����֑S�Ẵt�@�C�����R�s�[����܂��B<br/>
	''' �����֑S�Ẵt�@�C�����R�s�[�������Ȃ��Ƃ��́A<see cref="DestinationFiles"/> �łP�΂P�Ŏw�肷�邩�A<see cref="RemoveRoot"/> ���w�肵�Ă��������B<br/>
	''' </remarks>
	Public Property DestinationFolder() As String
		Get
			Return Me._destinationFolder
		End Get
		Set(ByVal value As String)
			' �Ō�́u���v�͏���
			If value.EndsWith(Path.DirectorySeparatorChar) Then
				value = value.Substring(0, value.Length - 1)
			End If
			Me._destinationFolder = value
		End Set
	End Property

	''' <summary>
	''' �R�s�[���̃t�@�C���������菜���p�X��
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ��\�� String �^�̃p�����[�^�ł��B<br/>
	''' �R�s�[���̃t�@�C���������菜���p�X�����w�肵�܂��B<br/>
	''' �w�肳�ꂽ�Ƃ��́A��v�����p�X����艺�̃t�H���_���ێ������܂܃R�s�[����܂��B<br/>
	''' �ȗ����ꂽ�Ƃ��́A<see cref="DestinationFiles"/> ���́A<see cref="DestinationFolder" />�Ŏw�肳�ꂽ
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
	''' �t�@�C���̏㏑������
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ��\�� Boolean �^�̃p�����[�^�ł��B<br/>
	''' �R�s�[��t�H���_�Ƀt�@�C�������ɑ��݂��Ă����Ƃ��A�㏑�����邩�w�肵�܂��B<br/>
	''' <c>True</c> ���w�肷��Ə㏑�����܂��B�ȗ����� <c>False</c> �ƂȂ�㏑�����܂���B<br/>
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
	''' �t�@�C�����ǂݎ���p�������Ƃ��̈���
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ��\�� Boolean �^�̃p�����[�^�ł��B<br/>
	''' �t�@�C�����ǂݎ���p�Ƃ��ă}�[�N����Ă���Ƃ��A�㏑�����邩�w�肵�܂��B<br/>
	''' <c>True</c> ���w�肷��Ə㏑�����܂��B�ȗ����� <c>False</c> �ƂȂ�㏑�����܂���B<br/>
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
	''' �ȗ��\�� Boolean �^�̃p�����[�^�ł��B<br/>
	''' <c>True</c> �ɐݒ肷��ƁA�R�s�[���̃t�@�C���ƃR�s�[��̃t�@�C���ŕύX���Ȃ��ꍇ�A�R�s�[�������X�L�b�v����܂��B<br/>
	''' <see cref="Copy"/> �^�X�N�ł́A�t�@�C���̃T�C�Y���������A�ŏI�X�V�������������ꍇ�A�t�@�C���͕ύX����Ă��Ȃ��ƌ��Ȃ���܂��B<br/>
	''' �ȗ����� <c>False</c> �ƂȂ�X�L�b�v���܂���B<br/>
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
	''' ���ۂɃR�s�[�����t�@�C��
	''' </summary>
	''' <value></value>
	''' <returns></returns>
	''' <remarks>
	''' �ȗ��\�� ITaskItem �^�z��̏o�̓p�����[�^�ł��B<br/>
	''' ����ɃR�s�[���ꂽ�A�C�e�����i�[����܂��B<br/>
	''' <see cref="SkipUnchanged"/> �� <c>True</c> �̂Ƃ��ɃR�s�[�����Ȃ������t�@�C���͏����܂��B<br/>
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
	''' �^�X�N�����s���܂��B
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

				' �t�@�C�������݂��邩�H
				If File.Exists(dstFile) Then
					' �ǂݎ���p�t�@�C���Ώ�
					If _overwriteReadOnly Then
						If (File.GetAttributes(dstFile) And FileAttributes.ReadOnly) = FileAttributes.ReadOnly Then
							File.SetAttributes(dstFile, FileAttributes.Normal)
						End If
					End If
					' �ŏI�X�V�������قȂ�t�@�C���Ώ�
					If _skipUnchanged Then
						If File.GetLastWriteTime(srcFile) = File.GetLastWriteTime(dstFile) Then
							Continue While
						End If
					End If
				End If

				' �R�s�[��̃t�H���_�`�F�b�N���Ė�����΍쐬
				chkDir = Path.GetDirectoryName(dstFile)
				If Not Directory.Exists(chkDir) Then
					Directory.CreateDirectory(chkDir)
				End If

				' �t�@�C���R�s�[
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
