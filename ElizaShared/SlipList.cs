using Eliza.Cells;

namespace Eliza;

/// <summary>
/// Double-linked symmetric list
/// </summary>
public class SlipList
{
    public SlipList()
    {
        Header = new SlipCellHeader(this);
    }

    /// <summary>
    /// List header
    /// </summary>
    public SlipCellHeader Header { get; }

    /// <summary>
    /// Check if list is empty
    /// </summary>
    public bool IsEmpty => Header.Left == Header && Header.Right == Header;

    /// <summary>
    /// The top (leftmost) cell of the list
    /// </summary>
    public SlipCellBase? Top => Header.Right != Header ? Header.Right : null;

    /// <summary>
    /// The bottom (rightmost) cell of the list
    /// </summary>
    public SlipCellBase? Bottom => Header.Left != Header ? Header.Left : null;

    /// <summary>
    /// Attached description (parameter/value) list
    /// </summary>
    public SlipList? DescriptionList { get; private set; }

    /// <summary>
    /// Empties the list
    /// </summary>
    /// <returns>The emptied list</returns>
    public SlipList EmptyList()
    {
        Header.Left = Header.Right = Header;
        return this;
    }

    /// <summary>
    /// Appends a description list to the list
    /// </summary>
    /// <param name="descriptionList">List to be appended</param>
    /// <returns>Appended description list</returns>
    public SlipList MakeDescriptionList(SlipList? descriptionList = null)
    {
        DescriptionList = descriptionList ?? new SlipList();
        return DescriptionList;
    }

    /// <summary>
    /// Removes the description list
    /// </summary>
    public void DropDescriptionList()
    {
        DescriptionList = null;
    }

    /// <summary>
    /// Inserts new datum cell to the top of the list.
    /// </summary>
    /// <param name="datum">Datum to keep in the cell</param>
    public void NewTop(string datum)
    {
        Header.NewBottom(datum);
    }

    /// <summary>
    /// Appends a sublist to the top of the list.
    /// </summary>
    /// <param name="list">Sublist to append</param>
    public SlipList NewTop(SlipList list)
    {
        return Header.NewBottom(list);
    }

    /// <summary>
    /// Inserts new datum cell to the bottom of the list.
    /// </summary>
    /// <param name="datum">Datum to keep in the cell</param>
    public void NewBottom(string datum)
    {
        Header.NewTop(datum);
    }

    /// <summary>
    /// Appends a sublist to the bottom of the list.
    /// </summary>
    /// <param name="list">Sublist to append</param>
    public SlipList NewBottom(SlipList list)
    {
        return Header.NewTop(list);
    }

    /// <summary>
    /// Pops the topmost cell off the list and returns it
    /// </summary>
    /// <returns>The popped cell</returns>
    public SlipCellBase? PopTop()
    {
        var top = Header.Right;
        if (top == Header)
            return null;

        Header.Right = top.Right;
        Header.Right.Left = Header;

        return top;
    }

    /// <summary>
    /// Pops the bottommost cell off the list and returns it
    /// </summary>
    /// <returns>The popped cell</returns>
    public SlipCellBase? PopBottom()
    {
        var bottom = Header.Left;
        if (bottom == Header)
            return null;

        Header.Left = bottom.Left;
        Header.Left.Right = Header;

        return bottom;
    }

    /// <summary>
    /// Makes a copy of the list (all contained data is copied, all sublists become sublists
    /// of the copy list as well)
    /// </summary>
    /// <returns>Copy of the list</returns>
    public SlipList ListCopy()
    {
        var copy = new SlipList();
        var reader = GetSequentialReader();

        while (reader.SequenceLinearRight() is not SlipCellHeader)
            switch (reader.Cell)
            {
                case SlipCellDatum datumCell:
                    copy.NewBottom(datumCell.Datum);
                    break;
                case SlipCellListName listName:
                    copy.NewBottom(listName.List);
                    break;
            }

        if (DescriptionList != null)
            copy.MakeDescriptionList(DescriptionList.ListCopy());

        return copy;
    }

    /// <summary>
    /// Creates new list containing all the cells to the left (above) of <paramref name="cell"/>
    /// including the <paramref name="cell"/> itself. Cells are removed from current list
    /// </summary>
    /// <param name="cell">Borderline cell</param>
    /// <returns>Newly created list</returns>
    public SlipList NewListLeft(SlipCellBase? cell)
    {
        var result = new SlipList();
        if (cell is null)
            return result;

        var next = PopTop();

        while (next is not null && next != cell)
        {
            if (next is SlipCellDatum datumCell)
                result.NewBottom(datumCell.Datum);
            else
                result.NewBottom(((SlipCellListName) next).List);

            next = PopTop();
        }

        switch (cell)
        {
            case SlipCellDatum datum:
                result.NewBottom(datum.Datum);
                break;
            case SlipCellListName listName:
                result.NewBottom(listName.List);
                break;
        }

        return result;
    }

    /// <summary>
    /// Creates new list containing all the cells to the right (below) of <paramref name="cell"/>
    /// including the <paramref name="cell"/> itself. Cells are removed from current list
    /// </summary>
    /// <param name="cell">Borderline cell</param>
    /// <returns>Newly created list</returns>
    public SlipList NewListRight(SlipCellBase? cell)
    {
        var result = new SlipList();
        if (cell is null)
            return result;

        var next = PopBottom();

        while (next is not null && next != cell)
        {
            if (next is SlipCellDatum datumCell)
                result.NewTop(datumCell.Datum);
            else
                result.NewTop(((SlipCellListName) next).List);

            next = PopBottom();
        }

        switch (cell)
        {
            case SlipCellDatum datum:
                result.NewTop(datum.Datum);
                break;
            case SlipCellListName listName:
                result.NewTop(listName.List);
                break;
        }

        return result;
    }

    /// <summary>
    /// Takes the body (everything except header) of the <paramref name="list"/> and
    /// inserts it to the right of the bottom of this list. The <paramref name="list"/> is emptied.
    /// </summary>
    /// <param name="list">List to insert</param>
    /// <returns>Emptied <paramref name="list"/></returns>
    public SlipList InsertRight(SlipList list)
    {
        return Header.InsertLeft(list);
    }

    /// <summary>
    /// Takes the body (everything except header) of the <paramref name="list"/> and
    /// inserts it to the left of the top of this list. The <paramref name="list"/> is emptied.
    /// </summary>
    /// <param name="list">List to insert</param>
    /// <returns>Emptied <paramref name="list"/></returns>
    public SlipList InsertLeft(SlipList list)
    {
        return Header.InsertRight(list);
    }

    /// <summary>
    /// Gets new reader for the list
    /// </summary>
    /// <returns>New sequential reader instance for the list</returns>
    public SequentialReader GetSequentialReader()
    {
        return SequentialReader.Get(this);
    }

    #region Overrides of Object

    /// <inheritdoc />
    public override string ToString()
    {
        return SlipReader.ListPrint(this);
    }

    #endregion
}