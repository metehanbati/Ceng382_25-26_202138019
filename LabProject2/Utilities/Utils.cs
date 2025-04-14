using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace LabProject2.Utilities
{
    public sealed class Utils
    {
        // Singleton pattern implementation
        private static readonly Lazy<Utils> _instance = new Lazy<Utils>(() => new Utils());
        
        public static Utils Instance => _instance.Value;
        
        // Private constructor for singleton
        private Utils() { }
        
        
        public string ExportToJson<T>(IEnumerable<T> data, List<string>? selectedProperties = null)
        {
            // If no data, return empty array
            if (data == null || !data.Any())
            {
                return "[]";
            }
            
            // If no properties selected, export all properties
            if (selectedProperties == null || !selectedProperties.Any())
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                return JsonSerializer.Serialize(data, options);
            }
            
            // If specific properties selected, create new anonymous objects with only those properties
            var filteredData = data.Select(item =>
            {
                var type = typeof(T);
                var result = new Dictionary<string, object?>();
                
                foreach (var propName in selectedProperties)
                {
                    var property = type.GetProperty(propName);
                    if (property != null)
                    {
                        result[property.Name] = property.GetValue(item);
                    }
                }
                
                return result;
            });
            
            var exportOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            return JsonSerializer.Serialize(filteredData, exportOptions);
        }
    }
}