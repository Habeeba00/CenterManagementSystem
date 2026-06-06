using CenterManagement.Domain.Entities;
using CenterManagement.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class GradeLevelController : Controller
    {
        private readonly CenterManagementDbContext _context;

        public GradeLevelController(CenterManagementDbContext context)
        {
            _context = context;
        }

        // GET: GradeLevel
        public async Task<IActionResult> Index()
        {
            var items = await _context.GradeLevels.OrderBy(x => x.Name).ToListAsync();
            return View(items);
        }

        // GET: GradeLevel/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: GradeLevel/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name")] GradeLevel gradeLevel)
        {
            if (ModelState.IsValid)
            {
                _context.Add(gradeLevel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Grade Level created successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(gradeLevel);
        }

        // GET: GradeLevel/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var gradeLevel = await _context.GradeLevels.FindAsync(id);
            if (gradeLevel == null) return NotFound();

            return View(gradeLevel);
        }

        // POST: GradeLevel/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] GradeLevel gradeLevel)
        {
            if (id != gradeLevel.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gradeLevel);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Grade Level updated successfully.";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GradeLevelExists(gradeLevel.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(gradeLevel);
        }

        // POST: GradeLevel/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hasCourses = await _context.Courses.AnyAsync(c => c.GradeLevelId == id);
            if (hasCourses)
            {
                TempData["Error"] = "Cannot delete this grade level because it is linked to one or more active courses.";
                return RedirectToAction(nameof(Index));
            }

            var gradeLevel = await _context.GradeLevels.FindAsync(id);
            if (gradeLevel != null)
            {
                gradeLevel.IsDeleted = true;
                _context.Update(gradeLevel);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Grade Level deleted successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool GradeLevelExists(int id)
        {
            return _context.GradeLevels.Any(e => e.Id == id);
        }
    }
}
