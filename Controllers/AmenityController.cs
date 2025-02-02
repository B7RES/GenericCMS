using Application.Common.Interfaces;
using Application.Common.Utility;
using Domain.Entities;
using Infrastructure.Migrations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Web.Models.ViewModels;

namespace Web.Controllers
{

    [Authorize(Roles = SD.Role_Admin)]
    public class AmenityController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AmenityController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            var amenities = _unitOfWork.Amenity.GetAll();
            return View(amenities);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var obj = new AmenityVM()
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                })
            };
            return View(obj);
        }

        [HttpPost]
        public IActionResult Create(AmenityVM obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Add(obj.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The Amenity has been created successfully.";
                return RedirectToAction(nameof(Index));
            }

            obj.VillaList = _unitOfWork.Villa.GetAll().Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });


            return View(obj);
        }


        [HttpGet]
        public IActionResult Update(int? id)
        {
            var obj = new AmenityVM
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(a => a.Id == id)
            };

            if (obj is null)
            {
                return RedirectToAction("Error");
            }


            return View(obj);

        }

        [HttpPost]
        public IActionResult Update(AmenityVM obj)
        {

            if (ModelState.IsValid)
            {
                _unitOfWork.Amenity.Update(obj.Amenity);
                _unitOfWork.Save();
                TempData["success"] = "The Amenity has been updated successfully.";
                return RedirectToAction(nameof(Index));
            }

            obj.VillaList = _unitOfWork.Villa.GetAll().Select(v => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
            {
                Text = v.Name,
                Value = v.Id.ToString()
            });


            return View(obj);

        }
        [HttpGet]
        public IActionResult Delete(int? id)
        {

            var obj = new AmenityVM
            {
                VillaList = _unitOfWork.Villa.GetAll().Select(v => new SelectListItem
                {
                    Text = v.Name,
                    Value = v.Id.ToString()
                }),
                Amenity = _unitOfWork.Amenity.Get(a => a.Id == id)
            };

            if (obj is null)
            {
                return RedirectToAction("Error");
            }

            return View(obj);

        }

        [HttpPost]
        public IActionResult Delete(AmenityVM obj)
        {
            var amenityDb = _unitOfWork.Amenity.Get(v => v.Id == obj.Amenity.Id);
            if (amenityDb is not null)
            {
                _unitOfWork.Amenity.Remove(amenityDb);
                _unitOfWork.Save();
                TempData["success"] = "The amenity has been deleted successfully";
                return RedirectToAction("Index");

            }
            TempData["error"] = "Couldnt not be deleted";
            return View();
        }
    }
}
