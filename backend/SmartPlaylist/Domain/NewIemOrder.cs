using SmartPlaylist.Contracts;
using System;
using System.Linq;

namespace SmartPlaylist.Domain
{
    public class NewItemOrder
    {
        public LimitOrder OrderBy { get; set; }

        public bool HasSort => !(OrderBy is NoneLimitOrder);

        public NewItemOrder() { }
        public NewItemOrder(SmartPlaylistNewItemOrderDto dto)
        {
            OrderBy = dto.HasSort ? DefinedLimitOrders.All.FirstOrDefault(x =>
                                string.Equals(x.Name, dto.OrderBy, StringComparison.CurrentCultureIgnoreCase)) ??
                          SmartPlaylistLimit.None.OrderBy : new NoneLimitOrder();
        }
    }
}