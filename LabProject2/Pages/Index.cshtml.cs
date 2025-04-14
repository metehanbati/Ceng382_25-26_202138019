using LabProject2.Models;
using LabProject2.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace LabProject2.Pages
{
    public class IndexModel : PageModel
    {
        // --- Data Store ---
        private static List<ClassInformationModel> _classes = new List<ClassInformationModel>();
        private static int _nextId = 1;
        private static bool _dataInitialized = false; // Flag to generate data only once

        // --- Properties for Display & Binding ---
        public List<ClassInformationTable> DisplayClasses { get; private set; } = new List<ClassInformationTable>();

        [BindProperty]
        public ClassInformationModel InputModel { get; set; } = new ClassInformationModel();

        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; }

        // --- Properties for Filtering ---
        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        // --- Properties for Pagination ---
        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1; // Default to page 1
        public int PageSize { get; set; } = 5; // Show 5 items per page
        public int TotalPages { get; private set; }
        public int TotalRecords { get; private set; }
        
        // --- Properties for Column Selection ---
        [BindProperty]
        public List<ColumnSelection> AvailableColumns { get; set; } = new List<ColumnSelection>();

        // Static constructor to ensure data generation happens only once
        static IndexModel()
        {
            InitializeSyntheticData();
        }

        private static void InitializeSyntheticData()
        {
            // Prevent re-initialization if already done
            if (!_dataInitialized && !_classes.Any())
            {
                for (int i = 1; i <= 105; i++) // Generate over 100 records
                {
                    _classes.Add(new ClassInformationModel
                    {
                        Id = _nextId++,
                        ClassName = $"Class {i:D3}", // e.g., Class 001, Class 002
                        StudentCount = 15 + (i % 20), // Some variation
                        Description = $"Description for synthetic class {i}. This is a sample description for testing pagination."
                    });
                }
                _dataInitialized = true;
            }
        }

        public void OnGet()
        {
            // Initialize available columns if not already done
            if (!AvailableColumns.Any())
            {
                AvailableColumns = new List<ColumnSelection>
                {
                    new ColumnSelection { PropertyName = "ClassName", DisplayName = "Class Name", IsSelected = false },
                    new ColumnSelection { PropertyName = "StudentCount", DisplayName = "Student Count", IsSelected = false },
                    new ColumnSelection { PropertyName = "Description", DisplayName = "Description", IsSelected = false }
                };
            }
            
            // Start with the full list as a queryable source
            var query = _classes.AsQueryable();

            // 1. Apply Filtering
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                // Case-insensitive search on ClassName
                query = query.Where(c => c.ClassName != null && 
                                   c.ClassName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // 2. Get Total Count after filtering (for pagination calculation)
            TotalRecords = query.Count();

            // 3. Calculate Pagination Details
            TotalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);

            // Ensure CurrentPage is within valid range
            CurrentPage = Math.Max(1, Math.Min(CurrentPage, Math.Max(1, TotalPages)));

            // 4. Apply Pagination (Skip and Take)
            var paginatedData = query
                .OrderBy(c => c.Id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToList();

            // 5. Map to DisplayClasses (ClassInformationTable model)
            DisplayClasses = paginatedData.Select(c => new ClassInformationTable
            {
                Id = c.Id,
                ClassName = c.ClassName,
                StudentCount = c.StudentCount,
                Description = c.Description
            }).ToList();

            // Handle Edit Pre-fill
            if (EditId.HasValue)
            {
                var classToEdit = _classes.FirstOrDefault(c => c.Id == EditId.Value);
                if (classToEdit != null)
                {
                    InputModel = new ClassInformationModel
                    {
                        Id = classToEdit.Id,
                        ClassName = classToEdit.ClassName,
                        StudentCount = classToEdit.StudentCount,
                        Description = classToEdit.Description
                    };
                }
                else 
                { 
                    EditId = null; 
                }
            }
        }

        public IActionResult OnPostAdd()
        {
            if (ModelState.IsValid)
            {
                InputModel.Id = _nextId++;
                _classes.Add(new ClassInformationModel
                {
                    Id = InputModel.Id,
                    ClassName = InputModel.ClassName,
                    StudentCount = InputModel.StudentCount,
                    Description = InputModel.Description
                });
                ModelState.Clear();
                InputModel = new ClassInformationModel();
                
                // Redirect will trigger OnGet, applying current filter/page
                return RedirectToPage(new { SearchTerm = SearchTerm, CurrentPage = CurrentPage });
            }
            
            // If invalid, repopulate display data
            OnGet();
            return Page();
        }

        public IActionResult OnPostEdit()
        {
            if (!ModelState.IsValid)
            {
                OnGet();
                return Page();
            }

            var classToUpdate = _classes.FirstOrDefault(c => c.Id == InputModel.Id);
            if (classToUpdate != null)
            {
                classToUpdate.ClassName = InputModel.ClassName;
                classToUpdate.StudentCount = InputModel.StudentCount;
                classToUpdate.Description = InputModel.Description;
            }

            ModelState.Clear();
            InputModel = new ClassInformationModel();
            EditId = null;
            
            // Redirect with current filter/page context
            return RedirectToPage(new { SearchTerm = SearchTerm, CurrentPage = CurrentPage });
        }

        public IActionResult OnPostDelete(int id)
        {
            var classToDelete = _classes.FirstOrDefault(c => c.Id == id);
            if (classToDelete != null)
            {
                _classes.Remove(classToDelete);
            }
            
            // Redirect with current filter/page context
            return RedirectToPage(new { SearchTerm = SearchTerm, CurrentPage = CurrentPage });
        }
        
        // New handler for toggling column selection
        public IActionResult OnPostToggleColumn(string propertyName)
        {
            // We need to repopulate the columns and toggle the selected one
            InitializeColumns();
            
            var column = AvailableColumns.FirstOrDefault(c => c.PropertyName == propertyName);
            if (column != null)
            {
                column.IsSelected = !column.IsSelected;
            }
            
            // Repopulate table data
            OnGet();
            return Page();
        }
        
        // Helper to initialize columns
        private void InitializeColumns()
        {
            if (!AvailableColumns.Any())
            {
                AvailableColumns = new List<ColumnSelection>
                {
                    new ColumnSelection { PropertyName = "ClassName", DisplayName = "Class Name", IsSelected = false },
                    new ColumnSelection { PropertyName = "StudentCount", DisplayName = "Student Count", IsSelected = false },
                    new ColumnSelection { PropertyName = "Description", DisplayName = "Description", IsSelected = false }
                };
            }
        }
        
        // Export handlers for JSON
        public IActionResult OnPostExportAllJson()
        {
            // Get selected column names or null if none selected
            var selectedColumns = GetSelectedColumnNames();
            
            // Use the singleton Utils class to export all classes
            string jsonData = Utils.Instance.ExportToJson(_classes, selectedColumns);
            
            // Return as file download
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            return File(bytes, "application/json", "all_classes.json");
        }
        
        public IActionResult OnPostExportFilteredJson()
        {
            // Apply filtering to get data
            var query = _classes.AsQueryable();
            
            if (!string.IsNullOrWhiteSpace(SearchTerm))
            {
                query = query.Where(c => c.ClassName != null && 
                                  c.ClassName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }
            
            var filteredData = query.ToList();
            
            // Get selected column names
            var selectedColumns = GetSelectedColumnNames();
            
            // Use the singleton Utils class to export filtered classes
            string jsonData = Utils.Instance.ExportToJson(filteredData, selectedColumns);
            
            // Return as file download
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonData);
            return File(bytes, "application/json", "filtered_classes.json");
        }
        
        // Helper to get selected column names
        private List<string>? GetSelectedColumnNames()
        {
            // If no columns are explicitly selected, return null (meaning all columns)
            if (!AvailableColumns.Any(c => c.IsSelected))
            {
                return null;
            }
            
            // Otherwise return the names of selected columns
            return AvailableColumns
                .Where(c => c.IsSelected)
                .Select(c => c.PropertyName)
                .ToList();
        }
    }
}