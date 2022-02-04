using SmartPlaylist.Contracts;
using System;
using System.Linq;

namespace SmartPlaylist.Domain
{
    public class NewItemOrder
    {
        public static readonly NewItemOrder None = new NewItemOrder
        {
            OrderBy = new NoneLimitOrder()
        };
        public LimitOrder OrderBy { get; set; }

        public bool HasSort => this != None;

        public NewItemOrder() { }
        public NewItemOrder(SmartPlaylistNewItemOrderDto dto)
        {
            OrderBy = dto.HasSort ? DefinedLimitOrders.All.FirstOrDefault(x =>
                                string.Equals(x.Name, dto.OrderBy, StringComparison.CurrentCultureIgnoreCase)) ??
                          SmartPlaylistLimit.None.OrderBy : new NoneLimitOrder();
        }
    }
}