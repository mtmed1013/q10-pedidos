using Inventory.Worker.Entities;

namespace Inventory.Worker.Services
{
    public class InventoryValidator
    {
        public static bool ValidateStock(Stock? stock, int cantidad)
        {
            if (stock == null || stock.Disponible < cantidad)
            {
                return false;
            }
            return true;
        }
    }
}