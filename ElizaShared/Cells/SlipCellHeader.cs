namespace Eliza.Cells;

/// <summary>
/// List Header cell
/// </summary>
public class SlipCellHeader : SlipCellBase
{
    public SlipCellHeader(SlipList list)
    {
        List = list;
    }

    /// <summary>
    /// The list this cell is the header of
    /// </summary>
    public SlipList List { get; }

    public override void Remove()
    {
        Console.Error.WriteLine("Attempt to remove the list header - operation cancelled.");
    }

    public override void Substitute(string datum)
    {
        _ = new SlipCellDatum(datum) { Left = Left, Right = Right };
        Left = Right = this;
    }

    /// <inheritdoc />
    public override void Substitute(SlipList list)
    {
        _ = new SlipCellListName(list) { Left = Left, Right = Right };
        Left = Right = this;
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return List.ToString();
    }

    #endregion
}