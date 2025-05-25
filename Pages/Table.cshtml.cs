using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Http;
using LabProject2.Models;
using LabProject2.Utilities; // Utils sınıfı için
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;

namespace LabProject2.Pages
{
    public class TableModel : PageModel
    {
        // Statik liste, in-memory veritabanı görevi görür (Week 5)
        private static List<ClassInformationModel> _classData = new List<ClassInformationModel>();
        private static int _nextId = 1; // Otomatik artan ID için

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; } // Filtreleme için arama terimi (Week 6)

        [BindProperty(SupportsGet = true)]
        public int PageIndex { get; set; } = 1; // Mevcut sayfa numarası (Week 6)

        public int PageSize { get; set; } = 10; // Sayfa başına gösterilecek kayıt sayısı
        public PaginatedList<ClassInformationModel>? ClassInformation { get; set; } // Sayfalanmış veri

        [BindProperty]
        public ClassInformationModel CurrentClass { get; set; } = new ClassInformationModel(); // Form için (Week 5)
        
        [BindProperty(SupportsGet = true)]
        public int? EditId { get; set; } // Düzenleme modu için ID (Week 5)

        public string ErrorMessage { get; set; } = string.Empty;
        public string SuccessMessage { get; set; } = string.Empty;

        // JSON export için seçilen sütunlar (Week 7)
        [BindProperty(SupportsGet = true)]
        public List<string> SelectedColumns { get; set; } = new List<string>();


        public void OnGet()
        {
            // Kullanıcı oturum açmamışsa Login sayfasına yönlendir (Güvenlik)
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("username")))
            {
                Response.Redirect("/Login");
                return;
            }

            // Eğer düzenleme modundaysak, formu önceden doldur (Week 5)
            if (EditId.HasValue)
            {
                var classToEdit = _classData.FirstOrDefault(c => c.Id == EditId.Value);
                if (classToEdit != null)
                {
                    CurrentClass = classToEdit;
                }
            }
            
            // Eğer veri yoksa veya 100'den azsa sentetik veri oluştur (Week 6)
            if (!_classData.Any() || _classData.Count < 100)
            {
                GenerateSyntheticData(100);
            }

            // Filtreleme mantığı (Week 6)
            IQueryable<ClassInformationModel> classes = _classData.AsQueryable();

            if (!string.IsNullOrEmpty(SearchTerm))
            {
                classes = classes.Where(c => c.ClassName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                             c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
            }

            // Sayfalama mantığı (Week 6)
            ClassInformation = PaginatedList<ClassInformationModel>.Create(
                classes.OrderBy(c => c.Id), PageIndex, PageSize);

            // TempData'daki mesajları al (İsteğe bağlı, kullanıcı geri bildirimleri için)
            if (TempData.ContainsKey("SuccessMessage"))
            {
                SuccessMessage = TempData["SuccessMessage"]?.ToString() ?? string.Empty;
            }
            if (TempData.ContainsKey("ErrorMessage"))
            {
                ErrorMessage = TempData["ErrorMessage"]?.ToString() ?? string.Empty;
            }
        }

        // Add/Edit handler (Week 5)
        public IActionResult OnPostAdd()
        {
            if (!ModelState.IsValid)
            {
                // ModelState geçerli değilse, mevcut veriyi sayfaya geri gönder
                OnGet(); // Sayfalamayı ve filtrelemeyi tekrar hesapla
                return Page();
            }

            // ID zaten varsa bu bir düzenleme işlemi demektir
            if (CurrentClass.Id == 0) // Yeni kayıt
            {
                CurrentClass.Id = _nextId++;
                _classData.Add(CurrentClass);
                TempData["SuccessMessage"] = "Yeni kayıt başarıyla eklendi!";
            }
            else // Mevcut kaydı düzenle
            {
                var existingClass = _classData.FirstOrDefault(c => c.Id == CurrentClass.Id);
                if (existingClass != null)
                {
                    existingClass.ClassName = CurrentClass.ClassName;
                    existingClass.StudentCount = CurrentClass.StudentCount;
                    existingClass.Description = CurrentClass.Description;
                    TempData["SuccessMessage"] = "Kayıt başarıyla güncellendi!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Güncellenecek kayıt bulunamadı.";
                }
            }
            
            CurrentClass = new ClassInformationModel(); // Formu temizle
            EditId = null; // Düzenleme modundan çık
            return RedirectToPage(); // Sayfayı yenile ve OnGet'i tekrar tetikle
        }

        // Delete handler (Week 5)
        public IActionResult OnPostDelete(int id)
        {
            var classToRemove = _classData.FirstOrDefault(c => c.Id == id);
            if (classToRemove != null)
            {
                _classData.Remove(classToRemove);
                TempData["SuccessMessage"] = "Kayıt başarıyla silindi!";
            }
            else
            {
                TempData["ErrorMessage"] = "Silinecek kayıt bulunamadı.";
            }
            return RedirectToPage();
        }

        // Edit handler (Bu sadece formu doldurmak için, asıl güncelleme OnPostAdd içinde yapılır) (Week 5)
        public IActionResult OnPostEdit(int id)
        {
            EditId = id; // Düzenleme moduna geç
            OnGet(); // Formu doldurmak için OnGet'i tekrar çağır
            return Page(); // Sayfayı yeniden yükle
        }
        
        // JSON Export handler (Week 7)
        public IActionResult OnPostExportJson(string exportMode)
        {
            List<ClassInformationModel> dataToExport;

            if (exportMode == "filtered")
            {
                // Mevcut filtre uygulanmış tüm veriyi al (sayfalamadan bağımsız)
                IQueryable<ClassInformationModel> classes = _classData.AsQueryable();
                if (!string.IsNullOrEmpty(SearchTerm))
                {
                    classes = classes.Where(c => c.ClassName.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
                                                 c.Description.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase));
                }
                dataToExport = classes.ToList(); 
            }
            else // unfiltered
            {
                dataToExport = _classData.ToList(); // Tüm veriyi al
            }

            var utils = Utils.Instance;
            string jsonString = utils.ExportToJson(dataToExport, SelectedColumns);

            // JSON dosyasını indirmek için File() döndür
            return File(Encoding.UTF8.GetBytes(jsonString), "application/json", "class_data.json");
        }

        // Sentetik Veri Üretimi (Week 6)
        private void GenerateSyntheticData(int count)
        {
            if (_classData.Count >= count) return; 

            var random = new Random();
            for (int i = _classData.Count; i < count; i++)
            {
                _classData.Add(new ClassInformationModel
                {
                    Id = _nextId++,
                    ClassName = $"Sınıf {i + 1}",
                    StudentCount = random.Next(10, 50),
                    Description = $"Bu, Sınıf {i + 1} için örnek bir açıklamadır."
                });
            }
        }
    }
}