using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using pfebackend.DTOs;

namespace pfebackend.Mappings
{
    public sealed class ExpenseDtoMap : ClassMap<ExpenseDto>
    {
        public ExpenseDtoMap()
        {
            Map(m => m.Name).Name("Name").Default("Missing Product Name");
            Map(m => m.Amount).Name("Amount").Default(0);
            Map(m => m.Date)
                .Name("Date")
                .TypeConverter<FlexibleDateTimeConverter>()
                .Default(DateTime.Now);
            Map(m => m.Category)
                .Name("Category")
                .TypeConverter<CategoryConverter>()
                .Default(Category.Others);
        }
    }

    public class CategoryConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Category.Others;

            if (Enum.TryParse(typeof(Category), text, true, out var result))
                return result;

            return Category.Others;
        }
    }

    public class FlexibleDateTimeConverter : DefaultTypeConverter
    {
        private static readonly string[] DateFormats = new[]
        {
            "MM/dd/yyyy HH:mm",       // 03/25/2025 18:45
            "yyyy-MM-dd HH:mm:ss",    // 2025-03-25 18:45:00
            "MM/dd/yyyy",             // 03/25/2025
            "yyyy-MM-dd",             // 2025-03-25
            "dd/MM/yyyy HH:mm",       // 25/03/2025 18:45
            "yyyy/MM/dd",             // 2025/03/25
            "yyyy-MM-ddTHH:mm:ss",    // 2025-03-25T18:45:00
            "yyyy-MM-ddTHH:mm:ss.fff" // 2025-03-25T18:45:00.000
        };

        public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrWhiteSpace(text))
                return DateTime.Now;

            foreach (var format in DateFormats)
            {
                if (DateTime.TryParseExact(text, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    return date;
            }

            return DateTime.Now;
        }
    }
}
