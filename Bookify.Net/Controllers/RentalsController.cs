using Microsoft.AspNetCore.DataProtection;

namespace Bookify.Net.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDataProtector _dataProtector;

        public RentalsController(ApplicationDbContext context, IDataProtectionProvider dataProtector, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
        }

        public IActionResult Create(string sKey)
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _context.Subsecribers
                                        .Include(s => s.Subscriptions)
                                        .Include(s => s.Rentals)
                                        .ThenInclude(s => s.RentalCopies)
                                        .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (string.IsNullOrEmpty(errorMessage))
            {
                return View("NotAllowedRental", errorMessage);
            }


            var ViewModel = new RentalFormViewModel
            {
                SubsecriberKey = sKey,
                MaxAlowedCopies = maxAllowedCopies

            };


            return View(ViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubsecriberKey));

            var subscriber = _context.Subsecribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);

            if (!string.IsNullOrEmpty(errorMessage))
                return View("NotAllowedRental", errorMessage);

            var selectedCopies = _context.BookCopies
                .Include(c => c.Book)
                .Include(c => c.Rentals)
                .Where(c => model.SelectedCopies.Contains(c.SerialNumber))
                .ToList();

            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.bookCopy)
                .Where(r => r.SubsecriberId == subscriberId)
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.bookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvilableForRentel)
                    return View("NotAllowedRental", Errors.NotAvalibleForRental);

                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue))
                    return View("NotAllowedRental", Errors.CopyIsInRental);

                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return View("NotAllowedRental", $"This subscriber already has a copy for '{copy.Book.Title}' Book");

                copies.Add(new RentalCopy { bookCopyId = copy.id });
            }

            Rental rental = new()
            {
                RentalCopies = copies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return Ok();
        }



        public IActionResult GetCopyDetails(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.
                Include(a => a.Book).
                SingleOrDefault(c => c.SerialNumber.ToString() == model.Value && !c.IsDeleted && !c.Book.IsDeleted);

            if (copy is null)
                return NotFound(Errors.InvalidSerailNumber);

            if (!copy.IsAvailableForRental || !copy.Book.IsAvilableForRentel)
                return NotFound(Errors.NotAvalibleForRental);

            //Todo Check The Copy Is Not in Rental
            var CopyIsInRental = _context.RentalCopies.Any(c => c.bookCopyId == copy.id && !c.ReturnDate.HasValue);
            if (CopyIsInRental)
                return NotFound(Errors.CopyIsInRental);

            var ViewModel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_CopyDetails", ViewModel);
        }


        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subsecriber subscriber)
        {
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return (errorMessage: Errors.InActiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals.SelectMany(r => r.RentalCopies).Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }


    }
}
