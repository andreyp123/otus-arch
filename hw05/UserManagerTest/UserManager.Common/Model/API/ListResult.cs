using System;

namespace UserManager.Common.Model.API
{
    public class ListResult<ItemT>
    {
        public ItemT[] Items { get; set; }
        public int Total { get; set; }

        public ListResult()
        {
        }

        public ListResult(ItemT[] items, int total)
        {
            Items = items;
            Total = total;
        }
    }
}
