using BlazingPizza.Server.Models;
using BlazingPizza.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazingPizza.Server.Controllers
{
    [Route("orders")]
    [ApiController]
    public class OrdersController : Controller
    {
        private readonly PizzaStoreContext _db;

        public OrdersController(PizzaStoreContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<OrderWithStatus>>> GetOrders()
        {
            return null;
        }

        [HttpPost]
        public async Task<ActionResult<int>> PlaceOrder(Order order)
        {
            order.CreatedTime = DateTime.Now;
            order.DeliveryLocation = new LatLong(51.5001, -0.1239);

            // Enforce existence of Pizza.SpecialId and Topping.ToppingId
            // in the database - prevent the submitter from making up
            // new specials and toppings
            foreach (Pizza pizza in order.Pizzas)
            {
                pizza.SpecialId = pizza.Special.Id;
                pizza.Special = null;

                foreach (PizzaTopping topping in pizza.Toppings)
                {
                    topping.ToppingId = topping.Topping.Id;
                    topping.Topping = null;
                }
            }

            _db.Orders.Attach(order);
            await _db.SaveChangesAsync();

            // In the background, send push notifications if possible
            NotificationSubscription subscription = await _db.NotificationSubscriptions.Where(x => x.UserId == GetUserId()).SingleOrDefaultAsync();
            if (subscription != null)
            {
                _ = TrackAndSendNotificationsAsync(order, subscription);
            }

            return order.OrderId;
        }

        private async Task TrackAndSendNotificationsAsync(Order order, NotificationSubscription subscription)
        {
            // In a realistic case, some other backend process would track
            // order delivery progress and send us notifications when it
            // changes. Since we don't have any such process here, fake it.
            await Task.Delay(OrderWithStatus.PreparationDuration);
            await SendNotificationAsync(order, subscription, "Your order has been dispatched!");

            await Task.Delay(OrderWithStatus.DeliveryDuration);
            await SendNotificationAsync(order, subscription, "Your order is now delivered. Enjoy!");
        }

        private Task SendNotificationAsync(Order order, NotificationSubscription subscription, string v)
        {
            // This will be implemented later
            return Task.CompletedTask;
        }

        private string GetUserId() => HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}