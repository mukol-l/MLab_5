using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

[TestClass]
public class CallDatabaseOldTests
{
    private string testFilePath = "calls_test.txt";
    private CallDatabase db;

    [TestInitialize]
    public void Setup()
    {
        // Створюємо нову тестову базу даних перед кожним тестом
        if (File.Exists(testFilePath))
            File.Delete(testFilePath);

        db = new CallDatabase(testFilePath);
    }

    [TestCleanup]
    public void Cleanup()
    {
        // Видаляємо тестовий файл після тесту
        if (File.Exists(testFilePath))
            File.Delete(testFilePath);
    }

    [TestMethod]
    public void AddOld_ShouldAddNewCallInfo()
    {
        // arrange
        int initialCount = db.LoadAll().Count;
        var newCall = new CallInfo("0998887776", "Lifecell", DateTime.ParseExact("27.11.2025", "dd.MM.yyyy", null), 15, 30);

        // act
        db.Add(newCall);
        var allCalls = db.LoadAll();

        // assert
        Assert.AreEqual(initialCount + 1, allCalls.Count, "Кількість записів після додавання не збільшується");
        Assert.IsTrue(allCalls.Any(c => c.Number == "0998887776" && c.Operator == "Lifecell"), "Доданий запис не знайдено у базі");
    }
}
