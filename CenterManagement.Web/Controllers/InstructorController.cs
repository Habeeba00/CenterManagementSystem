using CenterManagement.Application.DTOs.Instructor;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Web.ViewModels.Instructor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InstructorController : Controller
    {
        private readonly IInstructorService _instructorService;
        private readonly CenterManagementDbContext _db;

        public InstructorController(IInstructorService instructorService, CenterManagementDbContext db)
        {
            _instructorService = instructorService;
            _db = db;
        }

        private string GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Instructor
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var instructors = await _instructorService.GetAllInstructorsAsync();
            return View(instructors);
        }

        // GET: /Instructor/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateInstructorViewModel
            {
                SubjectSelectList = await GetSubjectSelectList()
            };
            return View(model);
        }

        // POST: /Instructor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreateInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.SubjectSelectList = await GetSubjectSelectList();
                return View("Create", model);
            }

            try
            {
                var dto = new CreateInstructorDto
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Specialization = model.Specialization,
                    SubjectId = model.SubjectId,
                    Photo = model.Photo
                };

                await _instructorService.CreateInstructorAsync(dto, GetAdminId());
                TempData["Success"] = "Instructor created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.SubjectSelectList = await GetSubjectSelectList();
                return View("Create", model);
            }
        }

        // GET: /Instructor/Profile/{id}
        [HttpGet]
        public async Task<IActionResult> Profile(int id)
        {
            try
            {
                var profile = await _instructorService.GetInstructorProfileAsync(id);
                return View(profile);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: /Instructor/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var profile = await _instructorService.GetInstructorProfileAsync(id);
                var model = new UpdateInstructorViewModel
                {
                    FullName = profile.FullName,
                    Specialization = profile.Specialization,
                    SubjectId = await _db.Subjects.Where(s => s.Name == profile.SubjectName).Select(s => s.Id).FirstOrDefaultAsync(),
                    CurrentImagePath = profile.ImagePath,
                    SubjectSelectList = await GetSubjectSelectList()
                };
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Instructor/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, UpdateInstructorViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.SubjectSelectList = await GetSubjectSelectList();
                return View("Edit", model);
            }

            try
            {
                var dto = new UpdateInstructorDto
                {
                    FullName = model.FullName,
                    Specialization = model.Specialization,
                    SubjectId = model.SubjectId,
                    Photo = model.Photo
                };

                await _instructorService.UpdateInstructorAsync(id, dto, GetAdminId());
                TempData["Success"] = "Instructor updated successfully.";
                return RedirectToAction(nameof(Profile), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Instructor/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _instructorService.SoftDeleteAsync(id, GetAdminId());
                TempData["Success"] = "Instructor archived successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Instructor not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<SelectList> GetSubjectSelectList()
        {
            var subjects = await _db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
            return new SelectList(subjects, "Id", "Name");
        }
    }
}
