using LVTN_BE_COFFE.Domain.IServices;
using LVTN_BE_COFFE.Domain.VModel;
using LVTN_BE_COFFE.Infrastructures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LVTN_BE_COFFE.Domain.Services
{
    public class OrderService : ControllerBase, IOrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActionResult<OrderResponse>> CreateOrderAsync(OrderCreateVModel request)
        {
            if (request.OrderItems == null || !request.OrderItems.Any())
                return BadRequest("Order must contain at least one item.");

            var orderItems = new List<OrderItem>();
            decimal totalAmount = 0m;

            foreach (var item in request.OrderItems)
            {
                var product = await _context.Products.FindAsync(item.ProductId);
                if (product == null) return NotFound($"Product ID {item.ProductId} not found.");

                var orderItem = new OrderItem
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.PriceAtPurchase
                };

                totalAmount += orderItem.Subtotal;
                orderItems.Add(orderItem);
            }

            var order = new Order
            {
                UserId = request.UserId,
                ShippingAddress = request.ShippingAddress,
                ShippingMethod = request.ShippingMethod,
                VoucherCode = request.VoucherCode,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                OrderItems = orderItems
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(order));
        }

        public async Task<ActionResult<OrderResponse>> UpdateOrderAsync(OrderUpdateVModel request)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null) return NotFound("Order not found.");

            if (!string.IsNullOrEmpty(request.ShippingAddress))
                order.ShippingAddress = request.ShippingAddress;

            if (!string.IsNullOrEmpty(request.ShippingMethod))
                order.ShippingMethod = request.ShippingMethod;

            if (!string.IsNullOrEmpty(request.Status))
                order.Status = request.Status;

            order.UpdatedAt = DateTime.UtcNow;

            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return Ok(MapToResponse(order));
        }

        public async Task<ActionResult<bool>> DeleteOrderAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Order not found.");

            _context.OrderItems.RemoveRange(order.OrderItems);
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return Ok(true);
        }

        public async Task<ActionResult<OrderResponse>> GetOrderByIdAsync(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound("Order not found.");

            return Ok(MapToResponse(order));
        }

        public async Task<ActionResult<IEnumerable<OrderResponse>>> GetOrdersAsync(OrderFilterVModel filter)
        {
            var query = _context.Orders
                .Include(o => o.OrderItems)
                .AsQueryable();

            if(filter.UserId is not null)
                query = query.Where(o => o.UserId == filter.UserId);

            if (!string.IsNullOrEmpty(filter.Status))
                query = query.Where(o => o.Status == filter.Status);

            if (filter.FromDate.HasValue)
                query = query.Where(o => o.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(o => o.CreatedAt <= filter.ToDate.Value);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((filter.PageNumber - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return Ok(orders.Select(MapToResponse));
        }

        private static OrderResponse MapToResponse(Order order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                ShippingAddress = order.ShippingAddress,
                ShippingMethod = order.ShippingMethod,
                Status = order.Status,
                VoucherCode = order.VoucherCode,
                TotalAmount = order.TotalAmount,
                FinalAmount = order.FinalAmount,
                ItemCount = order.ItemCount,
                CreatedAt = order.CreatedAt,
                OrderItems = order.OrderItems?.Select(static i => new OrderItemResponse
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product?.Name ?? "",
                    Quantity = i.Quantity,
                    PriceAtPurchase = i.PriceAtPurchase,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }
    }
}
