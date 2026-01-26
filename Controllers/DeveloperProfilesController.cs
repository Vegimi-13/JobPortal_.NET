using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using JobPortal_ServerSide.Data;
using JobPortal_ServerSide.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using JobPortal_ServerSide.Models.ViewModels;

namespace JobPortal_ServerSide.Controllers
{
    [Authorize(Roles = "Admin,Developer")]
    public class DeveloperProfilesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeveloperProfilesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: DeveloperProfiles
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            if (page < 1) page = 1;
            pageSize = pageSize switch
            {
                < 1 => 10,
                > 50 => 50,
                _ => pageSize
            };

            IQueryable<DeveloperProfile> query = _context.DeveloperProfiles
                .Include(d => d.User)
                .Include(d => d.DeveloperSkills)
                    .ThenInclude(ds => ds.Skill);

            if (!isAdmin)
            {
                query = query.Where(d => d.UserId == userId);
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var items = await query
                .OrderByDescending(d => d.IsActive)
                .ThenBy(d => d.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalCount = totalCount;
            ViewBag.TotalPages = totalPages;

            return View(items);
        }


        // GET: DeveloperProfiles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.DeveloperProfiles == null)
            {
                return NotFound();
            }

            var developerProfile = await _context.DeveloperProfiles
                .Include(d => d.User)
                .Include(d => d.DeveloperSkills)
                    .ThenInclude(ds => ds.Skill)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (developerProfile == null)
            {
                return NotFound();
            }

            return View(developerProfile);
        }

        // GET: DeveloperProfiles/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var exists = await _context.DeveloperProfiles
                .AnyAsync(d => d.UserId == userId);

            if (exists)
            {
                return RedirectToAction(nameof(Index));
            }

            var skills = await _context.Skills
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = s.Name
                })
                .ToListAsync();

            var vm = new CreateDeveloperProfileViewModel
            {
                Skills = skills
            };

            return View(vm);
        }

        // POST: DeveloperProfiles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDeveloperProfileViewModel model)
        {
            // Defensive: ensure skills exist in the dropdown when returning the view
            static async Task<List<SelectListItem>> LoadSkillsAsync(ApplicationDbContext context)
            {
                return await context.Skills
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToListAsync();
            }

            if (ModelState.IsValid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var developerProfile = new DeveloperProfile
                {
                    UserId = userId,
                    FullName = model.FullName,
                    Title = model.Title,
                    Bio = model.Bio,
                    Location = model.Location,
                    IsActive = true
                };

                _context.Add(developerProfile);
                await _context.SaveChangesAsync();

                var selectedSkillIds = model.SkillIds
                    .Distinct()
                    .ToList();

                if (selectedSkillIds.Count > 0)
                {
                    var validSkillIds = await _context.Skills
                        .Where(s => selectedSkillIds.Contains(s.Id))
                        .Select(s => s.Id)
                        .ToListAsync();

                    foreach (var skillId in validSkillIds)
                    {
                        _context.DeveloperSkills.Add(new DeveloperSkill
                        {
                            DeveloperProfileId = developerProfile.Id,
                            SkillId = skillId
                        });
                    }

                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            model.SkillIds ??= new List<int>();
            model.Skills = await LoadSkillsAsync(_context);
            return View(model);
        }

        // GET: DeveloperProfiles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var profile = await _context.DeveloperProfiles
                .Include(p => p.DeveloperSkills)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (profile == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (profile.UserId != userId)
                    return Forbid();
            }

            var skills = await _context.Skills
                .OrderBy(s => s.Name)
                .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                .ToListAsync();

            var vm = new EditDeveloperProfileViewModel
            {
                Id = profile.Id,
                FullName = profile.FullName,
                Title = profile.Title,
                Bio = profile.Bio,
                Location = profile.Location,
                IsActive = profile.IsActive,
                SkillIds = profile.DeveloperSkills.Select(ds => ds.SkillId).ToList(),
                Skills = skills
            };

            return View(vm);
        }


        // POST: DeveloperProfiles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditDeveloperProfileViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            var existing = await _context.DeveloperProfiles.AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existing == null) return NotFound();

            if (!User.IsInRole("Admin"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existing.UserId != userId)
                    return Forbid();
            }

            if (!ModelState.IsValid)
            {
                model.SkillIds ??= new List<int>();
                model.Skills = await _context.Skills
                    .OrderBy(s => s.Name)
                    .Select(s => new SelectListItem { Value = s.Id.ToString(), Text = s.Name })
                    .ToListAsync();

                return View(model);
            }

            var updated = new DeveloperProfile
            {
                Id = existing.Id,
                UserId = existing.UserId,
                FullName = model.FullName,
                Title = model.Title,
                Bio = model.Bio,
                Location = model.Location,
                IsActive = model.IsActive
            };

            _context.Update(updated);
            await _context.SaveChangesAsync();

            var selectedSkillIds = (model.SkillIds ?? new List<int>()).Distinct().ToList();
            var validSkillIds = await _context.Skills
                .Where(s => selectedSkillIds.Contains(s.Id))
                .Select(s => s.Id)
                .ToListAsync();

            var existingSkillIds = await _context.DeveloperSkills
                .Where(ds => ds.DeveloperProfileId == updated.Id)
                .Select(ds => ds.SkillId)
                .ToListAsync();

            var toRemove = existingSkillIds.Except(validSkillIds).ToList();
            var toAdd = validSkillIds.Except(existingSkillIds).ToList();

            if (toRemove.Count > 0)
            {
                var rows = await _context.DeveloperSkills
                    .Where(ds => ds.DeveloperProfileId == updated.Id && toRemove.Contains(ds.SkillId))
                    .ToListAsync();

                _context.DeveloperSkills.RemoveRange(rows);
            }

            foreach (var skillId in toAdd)
            {
                _context.DeveloperSkills.Add(new DeveloperSkill
                {
                    DeveloperProfileId = updated.Id,
                    SkillId = skillId
                });
            }

            if (toRemove.Count > 0 || toAdd.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: DeveloperProfiles/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.DeveloperProfiles == null)
            {
                return NotFound();
            }

            var developerProfile = await _context.DeveloperProfiles
                .Include(d => d.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (developerProfile == null)
            {
                return NotFound();
            }

            return View(developerProfile);
        }

        // POST: DeveloperProfiles/Delete/5
        [Authorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.DeveloperProfiles == null)
            {
                return Problem("Entity set 'ApplicationDbContext.DeveloperProfiles'  is null.");
            }
            var developerProfile = await _context.DeveloperProfiles.FindAsync(id);
            if (developerProfile != null)
            {
                _context.DeveloperProfiles.Remove(developerProfile);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DeveloperProfileExists(int id)
        {
          return (_context.DeveloperProfiles?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
