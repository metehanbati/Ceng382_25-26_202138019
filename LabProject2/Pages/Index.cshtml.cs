using LabProject2.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

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
    }
}