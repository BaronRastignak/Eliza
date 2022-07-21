namespace Eliza.Cells;

/// <summary>
/// List cell containing included list name (link to the sublist that is the part of the list containing this cell)
/// </summary>
public class SlipCellListName : SlipCellBase
{
    public SlipCellListName(SlipList list)
    {
        List = list;
    }

    /// <summary>
    /// The list this cell points to
    /// </summary>
    public SlipList List { get; private set; }

    public override void Substitute(string datum)
    {
        _ = new SlipCellDatum(datum) { Left = Left, Right = Right };
        Left = Right = this;
    }

    /// <inheritdoc />
    public override void Substitute(SlipList list)
    {
        List = list;
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return List.ToString();
    }

    #endregion
}