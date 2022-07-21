namespace Eliza.Cells;

/// <summary>
/// List cell containing datum
/// </summary>
public class SlipCellDatum : SlipCellBase
{
    public SlipCellDatum(string datum)
    {
        Datum = datum;
    }

    /// <summary>
    /// Cell datum
    /// </summary>
    public string Datum { get; private set; }

    public override void Substitute(string datum)
    {
        Datum = datum;
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
        return Datum;
    }

    #endregion
}