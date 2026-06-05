using CenterManagement.Application.DTOs.Group;
using CenterManagement.Application.Interfaces;
using CenterManagement.Web.ViewModels.Group;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace CenterManagement.Web.Controllers
{
    [Authorize(Roles = "Admin,Instructor")]
    public class GroupController : Controller
    {
        private readonly IGroupService _groupService;
        private readonly IInstructorService _instructorService;
        private readonly ICourseService _courseService;

        public GroupController(IGroupService groupService, IInstructorService instructorService, ICourseService courseService)
        {
            _groupService = groupService;
            _instructorService = instructorService;
            _courseService = courseService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // GET: /Group
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<GroupListItemDto> groups;
            if (User.IsInRole("Admin"))
            {
                groups = await _groupService.GetAllGroupsAsync();
                
                var instructors = await _instructorService.GetAllInstructorsAsync();
                ViewData["InstructorSelectList"] = new SelectList(instructors.Where(i => i.IsActive).OrderBy(i => i.FullName), "InstructorProfileId", "FullName");
            }
            else
            {
                // Instructor view
                // We need the instructor profile id for the current user
                // Usually we'd have a service method for this, I'll assume we can use GetInstructorProfileIdByUserIdAsync from session or we can add it. 
                // Let's use the DB context or create a method in InstructorService. Wait, ISessionService has GetInstructorProfileIdByUserIdAsync.
                // Or we can just use _instructorService if it had it. It doesn't, so I'll inject ISessionService or add it.
                // Let's inject ISessionService via HttpContext.RequestServices
                var sessionService = HttpContext.RequestServices.GetRequiredService<ISessionService>();
                var instructorProfileId = await sessionService.GetInstructorProfileIdByUserIdAsync(GetUserId());
                if (instructorProfileId == null) return Forbid();

                groups = await _groupService.GetGroupsByInstructorAsync(instructorProfileId.Value);
            }

            return View(groups);
        }

        // GET: /Group/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!User.IsInRole("Admin")) return Forbid();

            var model = new CreateGroupViewModel
            {
                CourseSelectList = await GetCourseSelectList(),
                InstructorSelectList = await GetInstructorSelectList()
            };
            return View(model);
        }

        // POST: /Group/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost(CreateGroupViewModel model)
        {
            if (!User.IsInRole("Admin")) return Forbid();

            if (!ModelState.IsValid)
            {
                model.CourseSelectList = await GetCourseSelectList();
                model.InstructorSelectList = await GetInstructorSelectList();
                return View("Create", model);
            }

            var dto = new CreateGroupDto
            {
                Name = model.Name,
                CourseId = model.CourseId,
                InstructorProfileId = model.InstructorProfileId,
                IsActive = model.IsActive
            };

            await _groupService.CreateGroupAsync(dto, GetUserId());
            TempData["Success"] = "Group created successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Group/Edit/{id}
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!User.IsInRole("Admin")) return Forbid();

            try
            {
                var group = await _groupService.GetGroupDetailAsync(id);
                var model = new UpdateGroupViewModel
                {
                    Name = group.Name,
                    IsActive = group.IsActive
                };
                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Group/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int id, UpdateGroupViewModel model)
        {
            if (!User.IsInRole("Admin")) return Forbid();

            if (!ModelState.IsValid)
            {
                return View("Edit", model);
            }

            try
            {
                var dto = new UpdateGroupDto
                {
                    Name = model.Name,
                    IsActive = model.IsActive
                };

                await _groupService.UpdateGroupAsync(id, dto, GetUserId());
                TempData["Success"] = "Group updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // POST: /Group/ChangeInstructor
        [HttpPost]
        public async Task<IActionResult> ChangeInstructor([FromBody] ChangeInstructorRequest request)
        {
            if (!User.IsInRole("Admin")) return Forbid();

            try
            {
                await _groupService.ChangeInstructorAsync(request.GroupId, request.InstructorProfileId, GetUserId());
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        private async Task<SelectList> GetCourseSelectList()
        {
            var courses = await _courseService.GetAllCoursesAsync(null, null);
            return new SelectList(courses, "Id", "Name");
        }

        private async Task<SelectList> GetInstructorSelectList()
        {
            var instructors = await _instructorService.GetAllInstructorsAsync();
            return new SelectList(instructors.Where(i => i.IsActive).OrderBy(i => i.FullName), "InstructorProfileId", "FullName");
        }
    }

    public class ChangeInstructorRequest
    {
        public int GroupId { get; set; }
        public int InstructorProfileId { get; set; }
    }
}
