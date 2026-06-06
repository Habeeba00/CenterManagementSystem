using CenterManagement.Application.DTOs.Course;
using CenterManagement.Application.Interfaces;
using CenterManagement.Infrastructure.Persistence;
using CenterManagement.Web.ViewModels.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly CenterManagementDbContext _db;

        public CourseController(ICourseService courseService, CenterManagementDbContext db)
        {
            _courseService = courseService;
            _db = db;
        }

        private string GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Course
        [HttpGet]
        public async Task<IActionResult> Index(int? gradeLevelId, int? subjectId)
        {
            var courses = await _courseService.GetAllCoursesAsync(gradeLevelId, subjectId);

            ViewData["GradeLevels"] = new SelectList(await _db.GradeLevels.AsNoTracking().OrderBy(g => g.Name).ToListAsync(), "Id", "Name", gradeLevelId);
            ViewData["Subjects"] = new SelectList(await _db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync(), "Id", "Name", subjectId);
            ViewData["CurrentGradeLevelId"] = gradeLevelId;
            ViewData["CurrentSubjectId"] = subjectId;

            return View(courses);
        }

        // GET: /Course/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new CreateCourseViewModel
            {
                GradeLevelSelectList = await GetGradeLevelSelectList(),
                SubjectSelectList = await GetSubjectSelectList()
            };
            return View(model);
        }

        // POST: /Course/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                model.SubjectSelectList = await GetSubjectSelectList();
                return View("Create", model);
            }

            try
            {
                var dto = new CreateCourseDto
                {
                    Name = model.Name,
                    Price = model.Price,
                    SubjectId = model.SubjectId,
                    GradeLevelId = model.GradeLevelId
                };

                await _courseService.CreateCourseAsync(dto, GetAdminId());
                TempData["Success"] = "Course created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                model.SubjectSelectList = await GetSubjectSelectList();
                return View("Create", model);
            }
        }

        // GET: /Course/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var course = await _courseService.GetCourseByIdAsync(id);
                var model = new UpdateCourseViewModel
                {
                    Name = course.Name,
                    Price = course.Price,
                    SubjectId = await _db.Subjects.Where(s => s.Name == course.SubjectName).Select(s => s.Id).FirstOrDefaultAsync(),
                    GradeLevelId = await _db.GradeLevels.Where(g => g.Name == course.GradeLevelName).Select(g => g.Id).FirstOrDefaultAsync(),
                    GradeLevelSelectList = await GetGradeLevelSelectList(),
                    SubjectSelectList = await GetSubjectSelectList()
                };
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Course/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, UpdateCourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.GradeLevelSelectList = await GetGradeLevelSelectList();
                model.SubjectSelectList = await GetSubjectSelectList();
                return View("Edit", model);
            }

            try
            {
                var dto = new UpdateCourseDto
                {
                    Name = model.Name,
                    Price = model.Price,
                    SubjectId = model.SubjectId,
                    GradeLevelId = model.GradeLevelId
                };

                await _courseService.UpdateCourseAsync(id, dto, GetAdminId());
                TempData["Success"] = "Course updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // GET: /Course/Delete/{id}
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var course = await _db.Courses.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
            if (course == null) return NotFound();
            return View(course);
        }

        // POST: /Course/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _courseService.SoftDeleteAsync(id, GetAdminId());
                TempData["Success"] = "Course archived successfully.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }
            catch (KeyNotFoundException)
            {
                TempData["Error"] = "Course not found.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<SelectList> GetGradeLevelSelectList()
        {
            var items = await _db.GradeLevels.AsNoTracking().OrderBy(g => g.Name).ToListAsync();
            return new SelectList(items, "Id", "Name");
        }

        private async Task<SelectList> GetSubjectSelectList()
        {
            var items = await _db.Subjects.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
            return new SelectList(items, "Id", "Name");
        }
    }
}
