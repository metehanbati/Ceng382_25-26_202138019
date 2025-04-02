using System.ComponentModel.DataAnnotations;

namespace LabProject2.Models
{
    public class ClassInformationModel
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Class Name is required")]
        [Display(Name = "Class Name")]
        public string ClassName { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Student Count is required")]
        [Range(1, 100, ErrorMessage = "Student Count must be between 1 and 100")]
        [Display(Name = "Student Count")]
        public int StudentCount { get; set; }
        
        [Required(ErrorMessage = "Description is required")]
        [StringLength(200, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 200 characters")]
        public string Description { get; set; } = string.Empty;
    }
}