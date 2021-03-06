﻿Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Microsoft.Build.Framework
Imports Microsoft.Build.Utilities



'''<summary>
'''CopyTest のテスト クラスです。すべての
'''CopyTest 単体テストをここに含めます
'''</summary>
<TestClass()> _
Public Class CopyTest


    Private testContextInstance As TestContext

    '''<summary>
    '''現在のテストの実行についての情報および機能を
    '''提供するテスト コンテキストを取得または設定します。
    '''</summary>
    Public Property TestContext() As TestContext
        Get
            Return testContextInstance
        End Get
        Set(value As TestContext)
            testContextInstance = Value
        End Set
    End Property

#Region "追加のテスト属性"
    '
    'テストを作成するときに、次の追加属性を使用することができます:
    '
    'クラスの最初のテストを実行する前にコードを実行するには、ClassInitialize を使用
    '<ClassInitialize()>  _
    'Public Shared Sub MyClassInitialize(ByVal testContext As TestContext)
    'End Sub
    '
    'クラスのすべてのテストを実行した後にコードを実行するには、ClassCleanup を使用
    '<ClassCleanup()>  _
    'Public Shared Sub MyClassCleanup()
    'End Sub
    '
    '各テストを実行する前にコードを実行するには、TestInitialize を使用
    '<TestInitialize()>  _
    'Public Sub MyTestInitialize()
    'End Sub
    '
    '各テストを実行した後にコードを実行するには、TestCleanup を使用
    '<TestCleanup()>  _
    'Public Sub MyTestCleanup()
    'End Sub
    '
#End Region


    '''<summary>
    '''Execute のテスト
    '''</summary>
    <TestMethod()> _
    Public Sub ExecuteTest()
        Dim lst As List(Of ITaskItem)
		Dim tsk As CopyEx
        Dim rc As Boolean

        lst = New List(Of ITaskItem)
        lst.Add(New TaskItem("..\..\data\hoge\**\*.*"))

		tsk = New CopyEx()
        tsk.SourceFiles = lst.ToArray
        tsk.DestinationFolder = "C:\Temp\MiYABiSTasksTest"
        tsk.RemoveRoot = My.Application.Info.DirectoryPath.Replace("\bin\Debug", String.Empty)
        tsk.Overwrite = True
        tsk.OverwriteReadOnly = True
        tsk.SkipUnchanged = True
        rc = tsk.Execute()

        Assert.IsTrue(rc)
        Assert.AreEqual(8, tsk.CopiedFiles.Length)
    End Sub
End Class
