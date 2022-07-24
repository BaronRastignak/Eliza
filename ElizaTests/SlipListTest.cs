using Eliza;
using Eliza.Cells;

namespace ElizaTests;

[TestClass]
public class SlipListTest
{
    [TestMethod]
    public void NewTop_SingleItem()
    {
        var p = new SlipList();
        p.NewTop("CAT");
        Assert.AreEqual("(CAT)", p.ToString());
    }

    [TestMethod]
    public void NewTop_NewBottom_SubListAndMultipleItems()
    {
        var a = new SlipList();
        a.NewTop("5");
        a.NewBottom("13");

        var b = a.NewTop(new SlipList());
        b.NewTop("10");

        Assert.AreEqual("((10) 5 13)", a.ToString());
    }

    [TestMethod]
    public void Top_DuplicateDatum()
    {
        var a = new SlipList();
        a.NewTop("CAT");
        a.NewTop((a.Top as SlipCellDatum)?.Datum!);
        Assert.AreEqual("(CAT CAT)", a.ToString());
    }

    [TestMethod]
    public void Top_ExtractSublist()
    {
        var a = new SlipList();
        a.NewTop("5");
        a.NewBottom("13");

        var b = a.NewTop(new SlipList());
        b.NewTop("10");

        var top = a.Top as SlipCellListName;
        var topTop = top?.List.Top as SlipCellDatum;
        Assert.AreEqual("10", topTop?.Datum);
    }

    [TestMethod]
    public void PopTop_MoveToBottom()
    {
        var a = new SlipList();
        a.NewBottom("5");
        a.NewBottom("10");
        a.NewBottom((a.PopTop() as SlipCellDatum)?.Datum!);
        Assert.AreEqual("(10 5)", a.ToString());
    }
}