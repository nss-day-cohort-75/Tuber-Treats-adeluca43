
using TuberTreats.Models;
using TuberTreats.Models.DTOs;

List<TuberDriver> deliveryDrivers = new List<TuberDriver>
{
    new TuberDriver { Id = 1, Name = "Bob" },
    new TuberDriver { Id = 2, Name = "Bobby" },
    new TuberDriver { Id = 3, Name = "Bonnie" }
};

List<Customer> customers = new List<Customer>
{
    new Customer { Id = 1, Name = "Alice", Address = "123 Main St" },
    new Customer { Id = 2, Name = "Sarah", Address = "456 Elm St" },
    new Customer { Id = 3, Name = "Charlie", Address = "789 Oak St" },
    new Customer { Id = 4, Name = "Diana", Address = "135 Maple Ave" },
    new Customer { Id = 5, Name = "Eli", Address = "246 Pine Rd" }
};

List<Topping> toppings = new List<Topping>
{
    new Topping { Id = 1, Name = "Cheese" },
    new Topping { Id = 2, Name = "Sour Cream" },
    new Topping { Id = 3, Name = "Bacon Bits" },
    new Topping { Id = 4, Name = "Chives" },
    new Topping { Id = 5, Name = "Butter" }
};

List<TuberOrder> orders = new List<TuberOrder>
{
    new TuberOrder
    {
        Id = 1,
        OrderPlacedOnDate = new DateTime(2025,4,23,10,0,0),
        CustomerId = 1,
        TuberDriverId = 1,
        DeliveredOnDate = null,
    },
    new TuberOrder
    {
        Id = 2,
        OrderPlacedOnDate = new DateTime(2025,4,23,11,0,0),
        CustomerId = 2,
        TuberDriverId = 2,
        DeliveredOnDate = null,
    },
    new TuberOrder
    {
        Id = 3,
        OrderPlacedOnDate = new DateTime(2025,4,23,09,0,0),
        CustomerId = 3,
        TuberDriverId = 3,
        DeliveredOnDate = null,
    }
};

List<TuberTopping> tuberToppings = new List<TuberTopping>
{
    new TuberTopping { Id = 1, TuberOrderId = 1, ToppingId = 1 },
    new TuberTopping { Id = 2, TuberOrderId = 1, ToppingId = 2 },
    new TuberTopping { Id = 3, TuberOrderId = 2, ToppingId = 3 },
     new TuberTopping { Id = 4, TuberOrderId = 3, ToppingId = 5 }
};


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); //redirect all HTTPs request to HTTPS for security 

app.UseAuthorization();

//add endpoints here

// get all tuber orders
app.MapGet("/tuberorders", () =>
{
    var orderDTOs = orders.Select(o => new TuberOrderDTO
    {
        Id = o.Id,
        OrderPlacedOnDate = o.OrderPlacedOnDate,
        DeliveredOnDate = o.DeliveredOnDate,
        CustomerId = o.CustomerId,
        TuberDriverId = o.TuberDriverId
    });

    return Results.Ok(orderDTOs);
});

//get tuber order by Id
app.MapGet("/tuberorders/{id}", (int id) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null) return Results.NotFound();

    var customer = customers.FirstOrDefault(c => c.Id == order.CustomerId);
    var driver = deliveryDrivers.FirstOrDefault(d => d.Id == order.TuberDriverId);

    var toppingDTO = tuberToppings
         .Where(tt => tt.TuberOrderId == order.Id)
         .Select(tt =>
         {
             var topping = toppings.FirstOrDefault(t => t.Id == tt.ToppingId);
             return new ToppingDTO
             {
                 Id = topping.Id,
                 Name = topping.Name
             };
         }).ToList();


    var newOrderDTO = new TuberOrderDTO
    {
        Id = order.Id,
        OrderPlacedOnDate = order.OrderPlacedOnDate,
        DeliveredOnDate = order.DeliveredOnDate,
        CustomerId = customer.Id,
        TuberDriverId = driver.Id,
    };
    var OrderWithToppings = new
    {
        Order = newOrderDTO,
        Toppings = toppingDTO
    };

    return Results.Ok(OrderWithToppings);
});




// submit a new order
app.MapPost("/tuberorders", (TuberOrder newOrder) =>
{

    int highestId = 0;
    foreach (TuberOrder order in orders)
    {
        if (order.Id > highestId)
        {
            highestId = order.Id;
        }
    }
    newOrder.Id = highestId + 1;
    newOrder.OrderPlacedOnDate = DateTime.Now;
    orders.Add(newOrder);

    return Results.Ok(newOrder);
});



//assign a driver
app.MapPut("/tuberorders/{id}", (int id, int driverId) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound();
    }

    var driver = deliveryDrivers.FirstOrDefault(d => d.Id == driverId);
    if (driver == null)
    {
        return Results.NotFound();
    }

    order.TuberDriverId = driverId;

    return Results.Ok(order);
});

// complete an order
app.MapPatch("/tuberorders/{id}/complete", (int id) =>
{
    var order = orders.FirstOrDefault(o => o.Id == id);
    if (order == null)
    {
        return Results.NotFound();
    }
    order.DeliveredOnDate = DateTime.Now;

    return Results.Ok(order);
});

// get all toppings
app.MapGet("/toppings", () =>
{
    var toppingDTOs = toppings.Select(t => new ToppingDTO
    {
        Id = t.Id,
        Name = t.Name
    });

    return Results.Ok(toppingDTOs);
});



// get topping by Id
app.MapGet("/toppings/{id}", (int id) =>
{
    var topping = toppings.FirstOrDefault(t => t.Id == id);
    if (topping == null) return Results.NotFound();

    var toppingNewDTO = new ToppingDTO
    {
        Id = topping.Id,
        Name = topping.Name
    };

    return Results.Ok(toppingNewDTO);
});

// get all tubertoppings 
app.MapGet("/tubertoppings", () =>
{
    var dtoTuberToppingsList = tuberToppings.Select(tuberTopping => new TuberToppingDTO
    {
        Id = tuberTopping.Id,
        TuberOrderId = tuberTopping.TuberOrderId,
        ToppingId = tuberTopping.ToppingId
    });

    return Results.Ok(dtoTuberToppingsList);
});



// add topping to tuberorder
app.MapPost("/tubertoppings", (TuberTopping newTuberTopping) =>
{
    int highestId = 0;
    foreach (TuberTopping topping in tuberToppings)
    {
        if (topping.Id > highestId)
        {
            highestId = topping.Id;
        }
    }

    newTuberTopping.Id = highestId + 1;

    tuberToppings.Add(newTuberTopping);

    var newTuberToppingDTO = new TuberToppingDTO
    {
        Id = newTuberTopping.Id,
        TuberOrderId = newTuberTopping.TuberOrderId,
        ToppingId = newTuberTopping.ToppingId
    };
    return Results.Ok(newTuberToppingDTO);
});



// remove a tuber topping
app.MapDelete("/tubertoppings/{id}", (int id) =>
{
    var toppingToRemove = tuberToppings.FirstOrDefault(tt => tt.Id == id);

    if (toppingToRemove == null)
    {
        return Results.NotFound();
    }

    tuberToppings.Remove(toppingToRemove);
    return Results.NoContent();
});


// get all customers
app.MapGet("/customers", () =>
{
    var customerDTOs = customers.Select(customer => new CustomerDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address
    });

    return Results.Ok(customerDTOs);
});

//get customer by id and their order
app.MapGet("/customers/{id}", (int id) =>
{
    var customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }
    var customerOrders = orders
       .Where(order => order.CustomerId == customer.Id)
       .Select(order => new TuberOrderDTO
       {
           Id = order.Id,
           OrderPlacedOnDate = order.OrderPlacedOnDate,
           DeliveredOnDate = order.DeliveredOnDate,
           CustomerId = order.CustomerId,
           TuberDriverId = order.TuberDriverId
       }).ToList();

    return Results.Ok(new CustomerWithOrdersDTO
    {
        Id = customer.Id,
        Name = customer.Name,
        Address = customer.Address,
        Orders = customerOrders
    }
    );
});

// add new customer
app.MapPost("/customers", (Customer newCustomer) =>
{

    int highestId = 0;
    foreach (Customer customer in customers)
    {
        if (customer.Id > highestId)
        {
            highestId = customer.Id;
        }
    }

    newCustomer.Id = highestId + 1;

    customers.Add(newCustomer);

    return Results.Ok(new CustomerDTO
    {
        Id = newCustomer.Id,
        Name = newCustomer.Name,
        Address = newCustomer.Address
    }
      );
});



// delete a customer

app.MapDelete("/customers/{id}", (int id) =>
{
    var customer = customers.FirstOrDefault(c => c.Id == id);

    if (customer == null)
    {
        return Results.NotFound();
    }

    customers.Remove(customer);
    return Results.NoContent();
});

// get all employees
app.MapGet("/tuberdeliveryDrivers", () =>
{
    var driverDTOs = deliveryDrivers.Select(driver => new TuberDriverDTO
    {
        Id = driver.Id,
        Name = driver.Name
    });

    return Results.Ok(driverDTOs);
});


// get employee by Id with their deliveries(orders)
app.MapGet("/tuberdeliveryDrivers/{id}", (int id) =>
{
    var driver = deliveryDrivers.FirstOrDefault(d => d.Id == id);
    if (driver == null)
    {
        return Results.NotFound();
    }

    //get driver's delivery orders
    var deliveries = orders
        .Where(order => order.TuberDriverId == driver.Id)
        .Select(order => new
        {
            order.Id,
            order.OrderPlacedOnDate,
            order.DeliveredOnDate
        });
    //make object with driver info and delivery orders 
    return Results.Ok(new
    {
        driver.Id,
        driver.Name,
        Deliveries = deliveries
    }
    );
});


app.Run();
//don't touch or move this!
public partial class Program { }