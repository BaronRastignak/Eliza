using Eliza;
using Eliza.Cells;

namespace ElizaTests;

[TestClass]
public class SlipReaderTest
{
    [TestMethod]
    public void ListReadTest()
    {
        const string input = "(A B DLIST (COLOR RED SIZE GIGANTIC) C (DONE DTWO (EONE ETWO)))";
        var car = new SlipList();
        using (var reader = new StringReader(input))
        {
            SlipReader.ListRead(car, reader);
        }

        var cell = car.Top;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("A", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("B", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("C", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellListName));

        var list = (cell as SlipCellListName)?.List;
        Assert.IsNotNull(list);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellHeader));

        cell = list.Top;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("DONE", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("DTWO", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellListName));

        list = (cell as SlipCellListName)?.List;
        Assert.IsNotNull(list);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellHeader));

        cell = list.Top;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("EONE", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("ETWO", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellHeader));

        list = car.DescriptionList;
        Assert.IsNotNull(list);

        cell = list.Top;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("COLOR", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("RED", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("SIZE", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellDatum));
        Assert.AreEqual("GIGANTIC", (cell as SlipCellDatum)?.Datum);

        cell = cell?.Right;
        Assert.IsInstanceOfType(cell, typeof(SlipCellHeader));
    }
}