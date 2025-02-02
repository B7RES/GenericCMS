using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class VillaController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public VillaController( IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment )
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;   // per fare upload
        }
        public IActionResult Index()
        {
            var villas = _unitOfWork.Villa.GetAll();
            return View(villas);
        }

        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        public IActionResult Create(Villa villa)
        {
            if(villa.Name == villa.Description)
            {
                ModelState.AddModelError("name", "Description cannot exactly match the Name.");
            }
            if (ModelState.IsValid)
            {
                if(villa.Image is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villa");

                    using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    {
                        villa.Image.CopyTo(fileStream);
                    }

                    villa.ImageUrl = @"\images\Villa\" + fileName;
                }
                else
                {
                    villa.ImageUrl = "https://placehold.co/600x400";
                }
                _unitOfWork.Villa.Add(villa);
                _unitOfWork.Save();

                return RedirectToAction("Index");
            }
            else
            {
                return View(villa);
            }
        }


        public IActionResult Update(int? id)
        {

            Villa? villa = _unitOfWork.Villa.Get(v => v.Id == id);
            if(villa == null)
            {
                return RedirectToAction("Error");
            }

            return View(villa);
        }



        [HttpPost]
        public IActionResult Update(Villa villa)
        {
            if (ModelState.IsValid)
            {

                if (villa.Image is not null)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(villa.Image.FileName);
                    string imagePath = Path.Combine(_webHostEnvironment.WebRootPath, @"images\Villa");

                    if (!string.IsNullOrEmpty(villa.ImageUrl))
                    {
                        var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, villa.ImageUrl.TrimStart('\\'));

                        if (System.IO.File.Exists(oldImagePath))
                        {
                            System.IO.File.Delete(oldImagePath);
                        }
                    }

                    using (var fileStream = new FileStream(Path.Combine(imagePath, fileName), FileMode.Create))
                    {
                        villa.Image.CopyTo(fileStream);
                    }

                    villa.ImageUrl = @"\images\Villa\" + fileName;
                }

                _unitOfWork.Villa.Update(villa);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult Delete(int? id)
        {
            Villa? villa = _unitOfWork.Villa.Get(v => v.Id == id);
            if (villa == null)
            {
                return RedirectToAction("Error");
            }

            return View(villa);
        }



        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villaDb = _unitOfWork.Villa.Get(v => v.Id == villa.Id);
            if (villaDb is not null)
            {
                _unitOfWork.Villa.Remove(villaDb);
                _unitOfWork.Save();
                TempData["success"] = "The villa has been deleted successfully";
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
