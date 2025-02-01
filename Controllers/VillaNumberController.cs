using Application.Common.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;

        public VillaNumberController(IUnitOfWork unitOfWork )
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            var villaNumbers = _unitOfWork.VillaNumber.GetAll();
            return View(villaNumbers);
        }

        [HttpGet]
        public IActionResult Create()
        {

            IEnumerable<SelectListItem> list = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
            bool roomNumberExists = _unitOfWork.VillaNumber.Any(VillaNumber => VillaNumber.Villa_Number == obj.VillaNumber.Villa_Number);

            if (ModelState.IsValid && !roomNumberExists)
            {
                _unitOfWork.VillaNumber.Add(obj.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            if (roomNumberExists)
            {
                TempData["error"] = "The villa Number already exists.";
            }
            obj.VillaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
                VillaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(v => v.Villa_Number == villaNumberId)
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
                _unitOfWork.VillaNumber.Update(obj.VillaNumber);
                _unitOfWork.Save();
                TempData["success"] = "The villa Number has been created successfully.";
                return RedirectToAction(nameof(Index));
            }
            
            obj.VillaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
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
                VillaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                VillaNumber = _unitOfWork.VillaNumber.Get(v => v.Villa_Number == villaNumberId)
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
            var villaNumberDb = _unitOfWork.VillaNumber.Get(v => v.Villa_Number == obj.VillaNumber.Villa_Number);
            if (villaNumberDb is not null)
            {
                _unitOfWork.VillaNumber.Remove(villaNumberDb);
                _unitOfWork.Save();
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
