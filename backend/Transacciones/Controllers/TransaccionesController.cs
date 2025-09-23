using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Transacciones.Data;
using Transacciones.Models;
using Transacciones.Services;

namespace Transacciones.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransaccionesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ProductoService _productoService;
    private readonly ILogger<TransaccionesController> _logger;

    public TransaccionesController(AppDbContext context, ProductoService productoService, ILogger<TransaccionesController> logger)
    {
        _context = context;
        _productoService = productoService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<Transaccion>>>> GetTransactions(
        [FromQuery] int? productId = null,
        [FromQuery] string? transactionType = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = "date",
        [FromQuery] string? sortDirection = "desc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Transacciones.AsQueryable();

            if (productId.HasValue)
                query = query.Where(t => t.ProductId == productId.Value);

            if (!string.IsNullOrEmpty(transactionType))
                query = query.Where(t => t.TransactionType == transactionType);

            if (dateFrom.HasValue)
                query = query.Where(t => t.TransactionDate >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(t => t.TransactionDate <= dateTo.Value.AddDays(1));

            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                query = query.Where(t => t.ProductName.ToLower().Contains(search) || (t.Details != null && t.Details.ToLower().Contains(search)));
            }

            query = sortBy?.ToLower() switch
            {
                "date" => sortDirection?.ToLower() == "asc"
                    ? query.OrderBy(t => t.TransactionDate)
                    : query.OrderByDescending(t => t.TransactionDate),
                "product" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.ProductName)
                    : query.OrderBy(t => t.ProductName),
                "type" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.TransactionType)
                    : query.OrderBy(t => t.TransactionType),
                "total" => sortDirection?.ToLower() == "desc"
                    ? query.OrderByDescending(t => t.TotalPrice)
                    : query.OrderBy(t => t.TotalPrice),
                _ => query.OrderByDescending(t => t.TransactionDate)
            };

            var totalRecords = await query.CountAsync();

            var transactions = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = new PagedResult<Transaccion>
            {
                Data = transactions,
                TotalRecords = totalRecords,
                Page = page,
                PageSize = pageSize
            };

            _logger.LogInformation("Retrieved {Count} transactions", transactions.Count);

            return Ok(new ApiResponse<PagedResult<Transaccion>>
            {
                Success = true,
                Message = "Transacciones obtenidas exitosamente",
                Data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transactions");
            return StatusCode(500, new ApiResponse<PagedResult<Transaccion>>
            {
                Success = false,
                Message = "Error al obtener transacciones",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<Transaccion>>> GetTransaction(int id)
    {
        try
        {
            var transaction = await _context.Transacciones.FindAsync(id);
            if (transaction == null)
            {
                return NotFound(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Transacción no encontrada"
                });
            }

            return Ok(new ApiResponse<Transaccion>
            {
                Success = true,
                Message = "Transacción obtenida exitosamente",
                Data = transaction
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transaction {TransactionId}", id);
            return StatusCode(500, new ApiResponse<Transaccion>
            {
                Success = false,
                Message = "Error al obtener transacción",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Transaccion>>> CreateTransaction(CreateTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = errors
                });
            }

            if (request.TransactionType != "Purchase" && request.TransactionType != "Sale")
            {
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Tipo de transacción inválido. Debe ser 'Purchase' o 'Sale'"
                });
            }

            var product = await _productoService.GetProductAsync(request.ProductId);
            if (product == null)
            {
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Producto no encontrado"
                });
            }

            if (request.TransactionType == "Sale" && product.Stock < request.Quantity)
            {
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = $"Stock insuficiente. Stock disponible: {product.Stock}, cantidad solicitada: {request.Quantity}"
                });
            }

            var transaction = new Transaccion
            {
                TransactionType = request.TransactionType,
                ProductId = request.ProductId,
                ProductName = product.Name,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                TotalPrice = request.Quantity * request.UnitPrice,
                Details = request.Details
            };

            var newStock = request.TransactionType == "Purchase"
                ? product.Stock + request.Quantity
                : product.Stock - request.Quantity;

            _logger.LogInformation("Updating stock for product {ProductId} from {OldStock} to {NewStock}",
                request.ProductId, product.Stock, newStock);

            var stockUpdated = await _productoService.UpdateStockAsync(request.ProductId, newStock);
            if (!stockUpdated)
            {
                _logger.LogError("Failed to update stock for product {ProductId}", request.ProductId);
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Error al actualizar el stock del producto"
                });
            }

            _context.Transacciones.Add(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction created with ID: {TransactionId} - Stock updated from {OldStock} to {NewStock}",
                transaction.Id, product.Stock, newStock);

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id },
                new ApiResponse<Transaccion>
                {
                    Success = true,
                    Message = "Transacción creada exitosamente",
                    Data = transaction
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, new ApiResponse<Transaccion>
            {
                Success = false,
                Message = "Error al crear transacción",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<Transaccion>>> UpdateTransaction(int id, UpdateTransactionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Datos de entrada inválidos",
                    Errors = errors
                });
            }

            var transaction = await _context.Transacciones.FindAsync(id);
            if (transaction == null)
            {
                return NotFound(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Transacción no encontrada"
                });
            }

            if (request.TransactionType != "Purchase" && request.TransactionType != "Sale")
            {
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Tipo de transacción inválido. Debe ser 'Purchase' o 'Sale'"
                });
            }

            var product = await _productoService.GetProductAsync(request.ProductId);
            if (product == null)
            {
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Producto no encontrado"
                });
            }

            var oldStock = transaction.TransactionType == "Purchase"
                ? product.Stock - transaction.Quantity
                : product.Stock + transaction.Quantity;

            var newStock = request.TransactionType == "Purchase"
                ? oldStock + request.Quantity
                : oldStock - request.Quantity;

            if (request.TransactionType == "Sale" && newStock < 0)
            {
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = $"Stock insuficiente para esta modificación. Stock resultante sería: {newStock}"
                });
            }

            _logger.LogInformation("Updating transaction {TransactionId} and stock for product {ProductId} from {OldStock} to {NewStock}",
                id, request.ProductId, product.Stock, newStock);

            var stockUpdated = await _productoService.UpdateStockAsync(request.ProductId, newStock);
            if (!stockUpdated)
            {
                _logger.LogError("Failed to update stock for product {ProductId}", request.ProductId);
                return BadRequest(new ApiResponse<Transaccion>
                {
                    Success = false,
                    Message = "Error al actualizar el stock del producto"
                });
            }


            transaction.TransactionType = request.TransactionType;
            transaction.ProductId = request.ProductId;
            transaction.ProductName = product.Name;
            transaction.Quantity = request.Quantity;
            transaction.UnitPrice = request.UnitPrice;
            transaction.TotalPrice = request.Quantity * request.UnitPrice;
            transaction.Details = request.Details;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction updated with ID: {TransactionId} - Stock updated to {NewStock}", id, newStock);

            return Ok(new ApiResponse<Transaccion>
            {
                Success = true,
                Message = "Transacción actualizada exitosamente",
                Data = transaction
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transaction {TransactionId}", id);
            return StatusCode(500, new ApiResponse<Transaccion>
            {
                Success = false,
                Message = "Error al actualizar transacción",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTransaction(int id)
    {
        try
        {
            var transaction = await _context.Transacciones.FindAsync(id);
            if (transaction == null)
            {
                return NotFound(new ApiResponse<bool>
                {
                    Success = false,
                    Message = "Transacción no encontrada"
                });
            }

            var product = await _productoService.GetProductAsync(transaction.ProductId);
            if (product != null)
            {

                var newStock = transaction.TransactionType == "Purchase"
                    ? product.Stock - transaction.Quantity
                    : product.Stock + transaction.Quantity;

                _logger.LogInformation("Deleting transaction {TransactionId} and reverting stock for product {ProductId} from {OldStock} to {NewStock}",
                    id, transaction.ProductId, product.Stock, Math.Max(0, newStock));

                await _productoService.UpdateStockAsync(transaction.ProductId, Math.Max(0, newStock));
            }

            _context.Transacciones.Remove(transaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Transaction deleted with ID: {TransactionId}", id);

            return Ok(new ApiResponse<bool>
            {
                Success = true,
                Message = "Transacción eliminada exitosamente",
                Data = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting transaction {TransactionId}", id);
            return StatusCode(500, new ApiResponse<bool>
            {
                Success = false,
                Message = "Error al eliminar transacción",
                Errors = new List<string> { ex.Message }
            });
        }
    }

    [HttpGet("products")]
    public async Task<ActionResult<ApiResponse<List<Product>>>> GetProducts()
    {
        try
        {
            var products = await _productoService.GetAllProductsAsync();
            _logger.LogInformation("Retrieved {Count} products for transactions", products.Count);

            return Ok(new ApiResponse<List<Product>>
            {
                Success = true,
                Message = "Productos obtenidos exitosamente",
                Data = products
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new ApiResponse<List<Product>>
            {
                Success = false,
                Message = "Error al obtener productos",
                Errors = new List<string> { ex.Message }
            });
        }
    }
}