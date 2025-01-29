using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VillaNumberController( ApplicationDbContext context )
        {
            _context = context;
        }
        public IActionResult Index()
        {
            var villaNumbers = _context.VillaNumbers
                .Include(v => v.Villa)
                .ToList();
            return View(villaNumbers);
        }

        [HttpGet]
        public IActionResult Create()
        {
        
            IEnumerable<SelectListItem> list = _context.Villas.ToList().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });

            VillaNumberVM villaNumberVM = new VillaNumberVM
            {
                VillaList = list
            };
            return View(villaNumberVM);
        }


        [HttpPost]
        public IActionResult Create(VillaNumberVM obj)
        {
            bool roomNumberExists = _context.VillaNumbers.Any(v => v.Villa_Number == obj.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomNumberExists)
            {
                _context.VillaNumbers.Add(obj.VillaNumber);
                _context.SaveChanges();
                TempData["success"] = "The villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (roomNumberExists)
            {
                TempData["error"] = "The villa Number already exists.";
            }
            obj.VillaList = _context.Villas.ToList().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });
            return View(obj);
        }


        [HttpGet]
        public IActionResult Update(int? villaNumberId)
        {
            VillaNumberVM obj = new VillaNumberVM
            {
                VillaList = _context.Villas.ToList().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = _context.VillaNumbers.FirstOrDefault(v => v.Villa_Number == villaNumberId)
            };

            if(obj.VillaNumber is null)
            {
                return RedirectToAction("Error");
            }
          

            return View(obj);
        }



        [HttpPost]
        public IActionResult Update(VillaNumberVM obj)
        {
            if (ModelState.IsValid)
            {
                _context.VillaNumbers.Update(obj.VillaNumber);
                _context.SaveChanges();
                TempData["success"] = "The villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            obj.VillaList = _context.Villas.ToList().Select(v => new SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });
            return View(obj);
        }


        [HttpGet]
        public IActionResult Delete(int? villaNumberId)
        {
            VillaNumberVM obj = new VillaNumberVM
            {
                VillaList = _context.Villas.ToList().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = _context.VillaNumbers.FirstOrDefault(v => v.Villa_Number == villaNumberId)
            };
            if (obj.VillaNumber is null)
            {
                return RedirectToAction("Error");
            }

            return View(obj);
        }



        [HttpPost]
        public IActionResult Delete(VillaNumberVM obj)
        {
            var villaNumberDb = _context.VillaNumbers.FirstOrDefault(v => v.Villa_Number == obj.VillaNumber.Villa_Number);
            if (villaNumberDb is not null)
            {
                _context.VillaNumbers.Remove(villaNumberDb);
                _context.SaveChanges();
                TempData["success"] = "The villa number has been deleted successfully";
                return RedirectToAction("Index");

            }
            TempData["error"] = "Couldnt not be deleted";
            return View();
        }


        public IActionResult Error()
        {
            return View();
        }

    }
}
