namespace GabriniCosmetics.Areas.Admin.Models.Interface
{
    public interface IOrder
    {
        public Task<Order> CreateOrder(Order order);
        public Task<Order> EditOrder(int id, Order order);
        public Task<OrderItems> CreateOrderItem(OrderItems orderItem);
        public Task<Order> GetLatestOrderForUser(string userId);
        public Task<Order> GetOrderByOrderId(int id);
        public Task<IEnumerable<Order>> GetOrdersByUserId(string userId);
        public Task<IEnumerable<Order>> GetOrders();
        public Task<IList<OrderItems>> GetOrderItemsByOrderId(int orderId);
        public Task<List<OrderItems>> GetOrderItems();
        public Task<OrderItems> UpdateOrderItems(OrderItems orderItems);
        public Task DeleteOrder(int id);
        public Task CreateOrderItems(IEnumerable<OrderItems> orderItems);
        Task<int> GetOrdersCountAsync();
        Task<int> GetPendingOrdersCountAsync();
        Task<int> GetAcceptsOrdersCountAsync();
        Task<int> GetRejectedOrdersCountAsync();
        Task<int> GetPaidCountAsync();
        Task<int> GetUnPaidCountAsync();


    }
}
