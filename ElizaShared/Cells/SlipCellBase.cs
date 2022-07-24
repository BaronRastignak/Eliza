using System.Reflection.PortableExecutable;

namespace Eliza.Cells;

/// <summary>
/// Base class for double-linked list cells
/// </summary>
public abstract class SlipCellBase
{
    protected SlipCellBase()
    {
        Left = Right = this;
    }

    /// <summary>
    /// Left (upper, previous) cell
    /// </summary>
    public SlipCellBase Left { get; set; }

    /// <summary>
    /// Right (lower, following) cell
    /// </summary>
    public SlipCellBase Right { get; set; }

    /// <summary>
    /// Inserts new datum cell on top of this cell.
    /// </summary>
    /// <param name="datum">Datum to keep in the cell</param>
    public void NewTop(string datum)
    {
        var cell = new SlipCellDatum(datum)
        {
            Left = Left,
            Right = this
        };

        Left.Right = cell;
        Left = cell;
    }

    /// <summary>
    /// Inserts a sublist on top of this cell.
    /// </summary>
    /// <param name="list">Sublist to append</param>
    public SlipList NewTop(SlipList list)
    {
        var cell = new SlipCellListName(list)
        {
            Left = Left,
            Right = this
        };

        Left.Right = cell;
        Left = cell;

        return cell.List;
    }

    /// <summary>
    /// Inserts new datum cell to the bottom of this cell.
    /// </summary>
    /// <param name="datum">Datum to keep in the cell</param>
    public void NewBottom(string datum)
    {
        var cell = new SlipCellDatum(datum)
        {
            Left = this,
            Right = Right
        };

        Right.Left = cell;
        Right = cell;
    }

    /// <summary>
    /// Inserts a sublist to the bottom of this cell.
    /// </summary>
    /// <param name="list">Sublist to append</param>
    public SlipList NewBottom(SlipList list)
    {
        var cell = new SlipCellListName(list)
        {
            Left = this,
            Right = Right
        };

        Right.Left = cell;
        Right = cell;

        return cell.List;
    }

    /// <summary>
    /// Takes the body (everything except header) of the <paramref name="list"/> and
    /// inserts it to the right of this cell. The <paramref name="list"/> is emptied.
    /// </summary>
    /// <param name="list">List to insert</param>
    /// <returns>Emptied <paramref name="list"/></returns>
    public SlipList InsertRight(SlipList list)
    {
        if (list.Top is null || list.Bottom is null)
            return list;

        list.Top.Left = this;
        list.Bottom.Right = Right;
        Right.Left = list.Bottom;
        Right = list.Top;

        return list.EmptyList();
    }

    /// <summary>
    /// Takes the body (everything except header) of the <paramref name="list"/> and
    /// inserts it to the left of this cell. The <paramref name="list"/> is emptied.
    /// </summary>
    /// <param name="list">List to insert</param>
    /// <returns>Emptied <paramref name="list"/></returns>
    public SlipList InsertLeft(SlipList list)
    {
        if (list.Top is null || list.Bottom is null)
            return list;

        list.Top.Left = Left;
        list.Bottom.Right = this;
        Left.Right = list.Top;
        Left = list.Bottom;

        return list.EmptyList();
    }

    /// <summary>
    /// Removes the cell from the list.
    /// </summary>
    public virtual void Remove()
    {
        Left.Right = Right;
        Right.Left = Left;
    }

    /// <summary>
    /// Substitutes what is stored in the cell with the <paramref name="datum"/>.
    /// </summary>
    /// <param name="datum">Substituting datum</param>
    public abstract void Substitute(string datum);

    /// <summary>
    /// Substitutes what is stored in the cell with the <paramref name="list"/> name.
    /// </summary>
    /// <param name="list">Substituting list</param>
    public abstract void Substitute(SlipList list);
}