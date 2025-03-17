using CsvHelper.Configuration;
using pfebackend.DTOs;

namespace pfebackend.Mappings
{
    public sealed class ExpenseDtoMap : ClassMap<ExpenseDto>
    {
        public ExpenseDtoMap()
        {
            Map(m => m.Name).Name("Name");
            Map(m => m.Category).Name("Category");
            Map(m => m.Date).Name("Date").TypeConverterOption.Format("dd/MM/yyyy HH:mm");
            Map(m => m.Amount).Name("Amount");
        }
    }
}