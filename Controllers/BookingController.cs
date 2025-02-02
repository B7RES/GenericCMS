using Application.Common.Interfaces;
using Application.Common.Utility;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Stripe.Checkout;
using System.Security.Claims;
using Web.Models.ViewModels;

namespace Web.Controllers
{
    public class BookingController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [Authorize]
        public IActionResult Index()
        {


            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            var user = _unitOfWork.User.Get(u => u.Id == userId);

            Booking booking = new()
            {
                VillaId = villaId,
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "Amenities"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Name = user.Name,
                Email = user.Email,
                Phone = user.PhoneNumber,
            };

            booking.TotalCost = booking.Villa.Price * nights;

          

            return View(booking);
        }



        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(Booking obj)
        {
            var villa = _unitOfWork.Villa.Get(u => u.Id == obj.VillaId);
            obj.TotalCost = villa.Price * obj.Nights;

            obj.Status = SD.StatusPending;

            obj.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(obj);
            _unitOfWork.Save();

            var domain = Request.Scheme + "://" + Request.Host.Value;
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>(),
                Mode= "payment",
                SuccessUrl = domain + $"/Booking/BookingConfirmation?bookingId={obj.Id}",
                CancelUrl = domain + $"/Booking/FinalizeBooking?villaId={obj.VillaId}&checkInDate={obj.CheckInDate}&nights={obj.Nights}",
            };


            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(obj.TotalCost * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        Description = villa.Description,
                        //Images = new List<string> { domain + villa.ImageUrl }, // e localhost quindi non posso
                    }
                },
                Quantity = 1,
            });

            var service = new SessionService();
            Session session = service.Create(options);

            _unitOfWork.Booking.UpdateStripePaymentID(obj.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            Booking obj = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

            if(obj.Status == SD.StatusPending)
            {

                var service = new SessionService();

                Session session = service.Get(obj.StripeSessionId);

                if (session.PaymentStatus == "paid")
                {
                    _unitOfWork.Booking.UpdateStatus(obj.Id, SD.StatusApproved);
                    _unitOfWork.Booking.UpdateStripePaymentID(obj.Id, session.Id, session.PaymentIntentId);
                    _unitOfWork.Save();
                }
            }

            return View(bookingId);
        }


        #region API CALLS
        [HttpGet]
        [Authorize]
        public IActionResult GetAll(string status)
        {
            IEnumerable<Booking> bookings;

            if (User.IsInRole(SD.Role_Admin))
            {
                bookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa");
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                bookings = _unitOfWork.Booking.GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if (!string.IsNullOrEmpty(status))
            {
                bookings = bookings.Where(b => b.Status.ToLower().Equals(status.ToLower()));
            }

            return Json(new { data = bookings });
        }

        #endregion
    }
}
