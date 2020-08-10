using BlazingPizza.Server.Models;
using BlazingPizza.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BlazingPizza.Server.Controllers
{
    [Route("toppings")]
    [ApiController]
    public class ToppingsController : Controller
    {
        private readonly PizzaStoreContext _db;

        public ToppingsController(PizzaStoreContext db)
        {
            _db = db;
        }

        public async Task<ActionResult<List<Topping>>> GetToppings()
            => await _db.Toppings.OrderBy(x => x.Name).ToListAsync();
    }
}