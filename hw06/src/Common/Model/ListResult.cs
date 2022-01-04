using System;

namespace Common.Model
{
    public class ListResult<ItemT>
    {
        public ItemT[] Items { get; set; }
        public int Total { get; set; }

        public ListResult()
            : this(new ItemT[0], 0)
        {
        }

        public ListResult(ItemT[] items, int total)
        {
            Items = items;
            Total = total;
        }
    }
}
