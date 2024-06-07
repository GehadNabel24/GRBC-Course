using Client.Models;
using Client.Proto;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Mvc;

namespace Client.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        // POST api/products - Adds or updates a product
        [HttpPost]
        public async Task<ActionResult> AddOrUpdateProduct(ProductModel product)
        {
            // Establish gRPC channel to the inventory service
            var channel = GrpcChannel.ForAddress("https://localhost:7115");
            var client = new Client.Proto.Inventory.InventoryClient(channel);

            // Check if the product exists by its ID
            var ExistRequest = new ProductIdRequest { Id = product.id };
            var ExistResponse = await client.GetProductByIdAsync(ExistRequest);

            // If response is not null, proceed
            if (ExistResponse != null)
            {
                // Prepare the product request to send
                var Request = new Product
                {
                    Id = product.id,
                    Title = product.title,
                    Price = product.price,
                    Quantity = product.quantity,
                    Category = (Category)product.category, // Map CategoryEnum to Category
                    ExpireDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(product.expireDate.ToUniversalTime())
                };

                // If product exists, update it
                if (ExistResponse.Exists == true)
                {
                    var UpdateResponse = await client.UpdateProductAsync(Request);
                    return Ok(new { Status = 201, Product = Request, Msg = UpdateResponse.Message });
                }
                else // If product does not exist, add it
                {
                    var InsertResponse = await client.AddProductAsync(Request);
                    return Created("", new { Status = 200, Product = Request, Msg = InsertResponse.Message });
                }
            }
            else
            {
                // Handle the case where response is null
                return NotFound(new { message = "Response For Product Equal Null" });
            }
        }

        // POST api/products/stream - Adds multiple products using streaming
        [HttpPost("Stream")]
        public async Task<IActionResult> AddBulkProducts([FromBody] List<ProductModel> products)
        {
            // Establish gRPC channel to the inventory service
            var channel = GrpcChannel.ForAddress("https://localhost:7115");
            var client = new Client.Proto.Inventory.InventoryClient(channel);
            var call = client.AddBulkProducts();

            // Stream each product to the server
            foreach (var product in products)
            {
                await call.RequestStream.WriteAsync(new Product
                {
                    Id = product.id,
                    Title = product.title,
                    Price = product.price,
                    Quantity = product.quantity,
                    Category = (Category)product.category, // Map CategoryEnum to Category
                    ExpireDate = Google.Protobuf.WellKnownTypes.Timestamp.FromDateTime(product.expireDate.ToUniversalTime())
                });
            }

            // Complete the stream and get the response
            await call.RequestStream.CompleteAsync();
            var response = await call;
            return Ok(new { message = "Products added successfully", count = response.InsertedCount });
        }

        // GET api/products/report - Generates a product report based on filter and order
        [HttpGet("Report")]
        public async Task<ActionResult<IEnumerable<Product>>> GenerateProductReport(bool priceOrder = false, int categoryFilter = (int)CategoryEnum.Not)
        {
            // Establish gRPC channel to the inventory service
            var channel = GrpcChannel.ForAddress("https://localhost:7115");
            var client = new Client.Proto.Inventory.InventoryClient(channel);
            var request = new ProductReportRequest
            {
                PriceOrder = priceOrder,
                CategoryFilter = (Category)categoryFilter // Map CategoryEnum to Category
            };

            var products = new List<Product>();
            var call = client.GetProductReport(request);

            // Read the response stream and add products to the list
            while (await call.ResponseStream.MoveNext())
            {
                var product = call.ResponseStream.Current;
                products.Add(product);
            }
            return Ok(products);
        }
    }
}
