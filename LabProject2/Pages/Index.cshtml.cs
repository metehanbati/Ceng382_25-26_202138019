using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabProject2.Models;


namespace LabProject2.Pages
{
    public class IndexModel : PageModel
    {
        private static List<ClassInformationModel> _classes = new List<ClassInformationModel>();
        private static int _nextId = 1;

        [BindProperty]
        public ClassInformationModel ClassInfo { get; set; } = new ClassInformationModel();

        public List<ClassInformationModel> Classes => _classes;

        public bool IsEditing { get; set; } = false;

        public void OnGet()
        {
            // Reset the form when the page is loaded
            ClassInfo = new ClassInformationModel();
        }

        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Set ID and add to the list
            ClassInfo.Id = _nextId++;
            _classes.Add(ClassInfo);

            // Reset the form
            ModelState.Clear();
            ClassInfo = new ClassInformationModel();

            return RedirectToPage();
        }

        public IActionResult OnGetEdit(int id)
        {
            var classToEdit = _classes.FirstOrDefault(c => c.Id == id);
            if (classToEdit != null)
            {
                ClassInfo = classToEdit;
                IsEditing = true;
            }

            return Page();
        }

        public IActionResult OnPostUpdate(int id)
        {
            if (!ModelState.IsValid)
            {
                IsEditing = true;
                return Page();
            }

            var classToUpdate = _classes.FirstOrDefault(c => c.Id == id);
            if (classToUpdate != null)
            {
                classToUpdate.ClassName = ClassInfo.ClassName;
                classToUpdate.StudentCount = ClassInfo.StudentCount;
                classToUpdate.Description = ClassInfo.Description;
            }

            // Reset the form
            ModelState.Clear();
            ClassInfo = new ClassInformationModel();
            IsEditing = false;

            return RedirectToPage();
        }

        public IActionResult OnGetDelete(int id)
        {
            var classToRemove = _classes.FirstOrDefault(c => c.Id == id);
            if (classToRemove != null)
            {
                _classes.Remove(classToRemove);
            }

            return RedirectToPage();
        }
    }
}
