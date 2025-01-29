using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaRepository _villaRepository;

        public VillaController( IVillaRepository villaRepository )
        {
            _villaRepository = villaRepository;
        }
        public IActionResult Index()
        {
            var villas = _villaRepository.GetAll();
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

                _villaRepository.Add(villa);
                _villaRepository.Save();

                return RedirectToAction("Index");
            }
            else
            {
                return View(villa);
            }
        }


        public IActionResult Update(int? id)
        {

            Villa? villa = _villaRepository.Get(v => v.Id == id);
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
                _villaRepository.Update(villa);
                _villaRepository.Save();
                TempData["success"] = "The villa has been updated successfully";
                return RedirectToAction("Index");
            }
            return View();
        }


        public IActionResult Delete(int? id)
        {
            Villa? villa = _villaRepository.Get(v => v.Id == id);
            if (villa == null)
            {
                return RedirectToAction("Error");
            }

            return View(villa);
        }



        [HttpPost]
        public IActionResult Delete(Villa villa)
        {
            var villaDb = _villaRepository.Get(v => v.Id == villa.Id);
            if (villaDb is not null)
            {
                _villaRepository.Remove(villaDb);
                _villaRepository.Save();
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
