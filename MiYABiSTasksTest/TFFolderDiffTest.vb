Imports System.Text
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Microsoft.Build.Utilities

''' <summary>
'''TFFolderDiff のテスト クラスです。すべての
'''TFFolderDiff 単体テストをここに含めます
''' </summary>
''' <remarks></remarks>
<TestClass()> Public Class TFFolderDiffTest

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
			testContextInstance = value
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
		Dim tsk As TFFolderDiff
		Dim rc As Boolean

		tsk = New TFFolderDiff()
		tsk.FolderSrc = "..\..\data\hoge"
        tsk.FolderDst = "..\..\data\hoge - コピー"
        tsk.Filter = "!*.log;!logs\"
		rc = tsk.Execute()

		Assert.IsTrue(rc)
        Assert.AreEqual(3, tsk.SrcOnlyFiles.Length)
        Assert.AreEqual(1, tsk.DstOnlyFiles.Length)
        Assert.AreEqual(1, tsk.SrcDifferentFiles.Length)
        Assert.AreEqual(1, tsk.DstDifferentFiles.Length)
    End Sub

	'''<summary>
	'''Execute のテスト
	'''</summary>
	<TestMethod()> _
	Public Sub ExecuteTest2()
		Dim tsk As TFFolderDiff
		Dim rc As Boolean

		tsk = New TFFolderDiff()
		tsk.FolderSrc = "..\..\data\hoge"
        tsk.FolderDst = "..\..\data\hoge - コピー"
        tsk.Filter = "!*.log;!logs\"
		tsk.FilterLocalPathsOnly = True
		rc = tsk.Execute()

		Assert.IsTrue(rc)
        Assert.AreEqual(3, tsk.SrcOnlyFiles.Length)
        Assert.AreEqual(1, tsk.DstOnlyFiles.Length)
        Assert.AreEqual(1, tsk.SrcDifferentFiles.Length)
        Assert.AreEqual(1, tsk.DstDifferentFiles.Length)
    End Sub

End Class