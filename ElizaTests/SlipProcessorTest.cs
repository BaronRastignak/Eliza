using Eliza;

namespace ElizaTests;

[TestClass]
public class SlipProcessorTest
{
    [TestMethod]
    public void YieldMatchTest()
    {
        var text = new SlipList();
        using (var reader = new StringReader("(MARY HAD A LITTLE LAMB ITS PROBABILITY WAS ZERO)"))
        {
            SlipReader.ListRead(text, reader);
        }

        var rule = new SlipList();
        using (var reader = new StringReader("(MARY 2 2 ITS 1 0)"))
        {
            SlipReader.ListRead(rule, reader);
        }

        var results = new SlipList();
        Assert.IsTrue(SlipProcessor.YieldMatch(rule, text, results));
        Assert.AreEqual("((MARY) (HAD A) (LITTLE LAMB) (ITS) (PROBABILITY) (WAS ZERO))", results.ToString());
    }

    [TestMethod]
    public void AssembleTest()
    {
        var results = new SlipList();
        using (var reader = new StringReader("((MARY) (HAD A) (LITTLE LAMB) (ITS) (PROBABILITY) (WAS ZERO))"))
        {
            SlipReader.ListRead(results, reader);
        }

        var spec = new SlipList();
        using (var reader = new StringReader("(DID 1 HAVE A 3)"))
        {
            SlipReader.ListRead(spec, reader);
        }

        var output = new SlipList();
        SlipProcessor.Assemble(spec, results, output);
        Assert.AreEqual("(DID MARY HAVE A LITTLE LAMB)", output.ToString());
    }

    [TestMethod]
    public void RuleTest()
    {
        var input = new SlipList();
        using (var reader = new StringReader("(MARY HAD A LITTLE LAMB ITS PROBABILITY WAS ZERO)"))
        {
            SlipReader.ListRead(input, reader);
        }

        var rule = new SlipList();
        using (var reader = new StringReader("(1 0 2 ITS 0 = DID 1 HAVE A 3)"))
        {
            SlipReader.ListRead(rule, reader);
        }

        var output = new SlipList();
        SlipProcessor.Rule(rule, input, output);
        Assert.AreEqual("(DID MARY HAVE A LITTLE LAMB)", output.ToString());
    }

    [DataTestMethod]
    [DataRow("ALWAYS", 7, 14)]
    [DataRow("HERE", 2, 3)]
    [DataRow("KIDS", 2, 1)]
    [DataRow("TIME", 2, 0)]
    public void HashTest(string datum, int bits, int expected)
    {
        Assert.AreEqual(expected, SlipProcessor.Hash(datum, bits));
    }
}