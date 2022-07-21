using Eliza.Cells;

namespace Eliza;

/// <summary>
/// Reader to read cells of the list in sequential order without descending into sublists
/// </summary>
public class SequentialReader
{
    private SequentialReader(SlipList list)
    {
        Cell = list.Header;
    }

    private SequentialReader(SlipCellBase cell)
    {
        Cell = cell;
    }

    /// <summary>
    /// Cell to which the reader points
    /// </summary>
    public SlipCellBase Cell { get; private set; }

    /// <summary>
    /// Gets new reader for the given list
    /// </summary>
    /// <param name="list">List to be sequentially read</param>
    /// <returns>New sequential reader instance for the list</returns>
    public static SequentialReader Get(SlipList list)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        return new SequentialReader(list);
    }

    /// <summary>
    /// Creates copy of the current reader attached to the same list.
    /// Copy may be moved through the list independently from the original reader.
    /// </summary>
    /// <returns>Copy of the reader pointing to the same cell as the original reader</returns>
    public SequentialReader ReaderCopy()
    {
        return new SequentialReader(Cell);
    }

    /// <summary>
    /// Moves reader to the next cell to the right
    /// </summary>
    /// <returns>New cell to which the reader points</returns>
    public SlipCellBase SequenceLinearRight()
    {
        Cell = Cell.Right;
        return Cell;
    }

    /// <summary>
    /// Moves reader to the next cell to the left
    /// </summary>
    /// <returns>New cell to which the reader points</returns>
    public SlipCellBase SequenceLinearLeft()
    {
        Cell = Cell.Left;
        return Cell;
    }

    /// <summary>
    /// Moves reader to the next cell to the right containing element datum
    /// </summary>
    /// <returns>Datum cell to which the reader points or <see langword="null"/> if there is no more elements in the list</returns>
    public SlipCellDatum? SequenceLinearElementRight()
    {
        do
        {
            Cell = Cell.Right;
        } while (Cell is not SlipCellDatum and not SlipCellHeader);

        return Cell as SlipCellDatum;
    }

    /// <summary>
    /// Moves reader to the next cell to the left containing element datum
    /// </summary>
    /// <returns>Datum cell to which the reader points or <see langword="null"/> if there is no more elements in the list</returns>
    public SlipCellDatum? SequenceLinearElementLeft()
    {
        do
        {
            Cell = Cell.Left;
        } while (Cell is not SlipCellDatum and not SlipCellHeader);

        return Cell as SlipCellDatum;
    }

    /// <summary>
    /// Moves reader to the next cell to the right containing sublist name
    /// </summary>
    /// <returns>Sublist name cell to which the reader points or <see langword="null"/> if there is no more elements in the list</returns>
    public SlipCellListName? SequenceLinearNameRight()
    {
        do
        {
            Cell = Cell.Right;
        } while (Cell is not SlipCellListName and not SlipCellHeader);

        return Cell as SlipCellListName;
    }

    /// <summary>
    /// Moves reader to the next cell to the left containing sublist name
    /// </summary>
    /// <returns>Sublist name cell to which the reader points or <see langword="null"/> if there is no more elements in the list</returns>
    public SlipCellListName? SequenceLinearNameLeft()
    {
        do
        {
            Cell = Cell.Left;
        } while (Cell is not SlipCellListName and not SlipCellHeader);

        return Cell as SlipCellListName;
    }
}