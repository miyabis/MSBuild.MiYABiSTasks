Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports Microsoft.Build.Framework
Imports Microsoft.Build.Utilities



'''<summary>
'''CreateItemTest のテスト クラスです。すべての
'''CreateItemTest 単体テストをここに含めます
'''</summary>
<TestClass()> _
Public Class CreateItemExTest


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
		Dim lst2 As List(Of ITaskItem)
		Dim tsk As CreateItemEx = New CreateItemEx()
		Dim rc As Boolean

		lst = New List(Of ITaskItem)
		lst.Add(New TaskItem("..\..\data\hoge\**\*.*"))
		lst2 = New List(Of ITaskItem)
        lst2.Add(New TaskItem("..\..\data\hoge\**\tes - コピー.txt"))

		tsk.Include = lst.ToArray
		tsk.Exclude = lst2.ToArray
		tsk.ReadOnly = False
		rc = tsk.Execute()

		Assert.IsTrue(rc)
        Assert.AreEqual(7, tsk.Include.Length)
    End Sub

End Class
