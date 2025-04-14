using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace LabProject2.Models
{
    public class ClassInformationTable
    {
        // ID is used for edit/delete operations but not displayed in the table
        public int Id { get; set; }
        
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;
        
        [Display(Name = "Student Count")]
        public int StudentCount { get; set; }
        
        public string Description { get; set; } = string.Empty;
        
        // Not part of the data model, but used for column selection in UI
        [JsonIgnore]
        public bool IsSelected { get; set; } = false;
    }
    
    // New class to represent available columns for selection
    public class ColumnSelection
    {
        public string PropertyName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public bool IsSelected { get; set; } = false;
    }
}