using System.ComponentModel.DataAnnotations;

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
    }
}