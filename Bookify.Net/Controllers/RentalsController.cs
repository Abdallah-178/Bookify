namespace Bookify.Net.Controllers
{
    [Authorize(Roles = AppRoles.Reception)]
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

        [HttpGet]
        //Validate subscriber (is null)
        //Validate subscriber (blacklist)
        public IActionResult Create(string sKey) //coming from subscriber detials sKey encrypted 
        {
            var subscriberId = int.Parse(_dataProtector.Unprotect(sKey));

            var subscriber = _context.Subsecribers
                                        .Include(s => s.Subscriptions)
                                        .Include(s => s.Rentals)
                                        .ThenInclude(s => s.RentalCopies)
                                        .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            //var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);


            //if (string.IsNullOrEmpty(errorMessage))
            //{
            //    return View("NotAllowedRental", errorMessage);
            //}


            if (subscriber.IsBlackListed)
                return View("NotAllowedRental", Errors.BlackListedSubscriber);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return View("NotAllowedRental", Errors.InActiveSubscriber);


            var currentRentals = subscriber.Rentals.SelectMany(r => r.RentalCopies).Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return View("NotAllowedRental", Errors.MaxCopiesReached);


            var ViewModel = new RentalFormViewModel
            {
                SubsecriberKey = sKey,
                MaxAlowedCopies = availableCopiesCount

            };


            return View("Form", ViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubsecriberKey));

            var subscriber = _context.Subsecribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            //Refactoring Code
            //var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber);
            //if (!string.IsNullOrEmpty(errorMessage))
            //    return View("NotAllowedRental", errorMessage);

            if (subscriber.IsBlackListed)
                return View("NotAllowedRental", Errors.BlackListedSubscriber);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return View("NotAllowedRental", Errors.InActiveSubscriber);


            var currentRentals = subscriber.Rentals.SelectMany(r => r.RentalCopies).Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return View("NotAllowedRental", Errors.MaxCopiesReached);




            var selectedCopies = _context.BookCopies // select * from users where id in (1,2,3)
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

            var rental = new Rental
            {
                RentalCopies = copies,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            subscriber.Rentals.Add(rental);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = rental.Id });

        }


        [HttpGet]
        public IActionResult Edit(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(bk => bk.bookCopy)
                .SingleOrDefault(r => r.Id == id);

            //if (rental is null || rental.CreatedOn.Date != DateTime.Today)  // must be in same day 
            //    return NotFound();

            var subscriber = _context.Subsecribers
                                       .Include(s => s.Subscriptions)
                                       .Include(s => s.Rentals)
                                       .ThenInclude(s => s.RentalCopies)
                                       .SingleOrDefault(s => s.Id == rental.SubsecriberId);


            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlackListed)
                return View("NotAllowedRental", Errors.BlackListedSubscriber);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return View("NotAllowedRental", Errors.InActiveSubscriber);


            var currentRentals = subscriber.Rentals
               .Where(r => r.Id != rental.Id || rental.Id == null)
               .SelectMany(r => r.RentalCopies)
               .Count(c => !c.ReturnDate.HasValue);


            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return View("NotAllowedRental", Errors.MaxCopiesReached);



            var currentcopiesIds = rental.RentalCopies.Select(c => c.bookCopyId).ToList(); //Get all Id for copies

            var CurrentCopies = _context.BookCopies
                .Where(c => currentcopiesIds.Contains(c.id))
                .Include(c => c.Book)
                .ToList();

            var ViewModel = new RentalFormViewModel
            {
                SubsecriberKey = _dataProtector.Protect(subscriber!.Id.ToString()),
                MaxAlowedCopies = availableCopiesCount,
                CurrentCopies = _mapper.Map<IEnumerable<BookCopyViewModel>>(CurrentCopies)
            };

            return View("Form", ViewModel);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(RentalFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", model);

            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                .SingleOrDefault(r => r.Id == model.Id);

            //if (rental is null || rental.CreatedOn.Date != DateTime.Today)
            //    return NotFound();

            var subscriberId = int.Parse(_dataProtector.Unprotect(model.SubsecriberKey));

            var subscriber = _context.Subsecribers
                .Include(s => s.Subscriptions)
                .Include(s => s.Rentals)
                .ThenInclude(r => r.RentalCopies)
                .SingleOrDefault(s => s.Id == subscriberId);

            if (subscriber is null)
                return NotFound();

            //Refactoring Code
            //var (errorMessage, maxAllowedCopies) = ValidateSubscriber(subscriber!, model.Id);

            if (subscriber.IsBlackListed)
                return View("NotAllowedRental", Errors.BlackListedSubscriber);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return View("NotAllowedRental", Errors.InActiveSubscriber);

            var currentRentals = subscriber.Rentals
             .Where(r => r.Id != rental.Id || rental.Id == null)
             .SelectMany(r => r.RentalCopies)
             .Count(c => !c.ReturnDate.HasValue);


            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return View("NotAllowedRental", Errors.MaxCopiesReached);


            var selectedCopies = _context.BookCopies
               .Include(c => c.Book)
               .Include(c => c.Rentals)
               .Where(c => model.SelectedCopies.Contains(c.SerialNumber))
               .ToList();

            var currentSubscriberRentals = _context.Rentals
                .Include(r => r.RentalCopies)
                .ThenInclude(c => c.bookCopy)
                .Where(r => r.SubsecriberId == subscriberId && r.Id != model.Id)
                .SelectMany(r => r.RentalCopies)
                .Where(c => !c.ReturnDate.HasValue)
                .Select(c => c.bookCopy!.BookId)
                .ToList();

            List<RentalCopy> copies = new();

            foreach (var copy in selectedCopies)
            {
                if (!copy.IsAvailableForRental || !copy.Book!.IsAvilableForRentel)
                    return View("NotAllowedRental", Errors.NotAvalibleForRental);

                if (copy.Rentals.Any(c => !c.ReturnDate.HasValue && c.RentalId != model.Id))
                    return View("NotAllowedRental", Errors.CopyIsInRental);

                if (currentSubscriberRentals.Any(bookId => bookId == copy.BookId))
                    return View("NotAllowedRental", $"This subscriber already has a copy for '{copy.Book.Title}' Book");

                copies.Add(new RentalCopy { bookCopyId = copy.id });
            }

            rental.RentalCopies = copies;
            rental.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            rental.LastUpdatedOn = DateTime.Now;


            _context.SaveChanges();
            return RedirectToAction(nameof(Details), new { id = rental.Id });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MarkIsDeleted(int id)
        {
            var rental = _context.Rentals
                         .Include(r => r.RentalCopies)
                         .SingleOrDefault(r => r.Id == id);

            if (rental is null || rental.CreatedOn.Date != DateTime.Today)
                return NotFound();

            rental.IsDeleted = true;
            rental.LastUpdatedOn = DateTime.Now;
            rental.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            var copiescount = rental.RentalCopies.Count(r => r.RentalId == id);


            _context.SaveChanges();
            return Ok(copiescount);
        }


        public IActionResult Details(int id)
        {
            var rental = _context.Rentals
                .Include(r => r.RentalCopies)
                    .ThenInclude(rc => rc.bookCopy)
                        .ThenInclude(bc => bc.Book)
                        .SingleOrDefault(r => r.Id == id && !r.IsDeleted);

            if (rental is null)
                return NotFound();

            var ViewModel = _mapper.Map<RentalViewModel>(rental);

            return View(ViewModel);
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


            if (!copy.IsAvailableForRental || !copy.Book!.IsAvilableForRentel)
                return BadRequest(Errors.NotAvalibleForRental);

            // Check The Copy Is Not in Rental

            var copyIsInRental = _context.RentalCopies.Any(c => c.bookCopyId == copy.id && !c.ReturnDate.HasValue);
            if (copyIsInRental)
                return BadRequest(Errors.CopyIsInRental);


            var ViewModel = _mapper.Map<BookCopyViewModel>(copy);


            return PartialView("_CopyDetails", ViewModel);
        }



        private (string errorMessage, int? maxAllowedCopies) ValidateSubscriber(Subsecriber subscriber, int? rentalId = null)
        {
            if (subscriber.IsBlackListed)
                return (errorMessage: Errors.BlackListedSubscriber, maxAllowedCopies: null);

            if (subscriber.Subscriptions.Last().EndDate < DateTime.Today.AddDays((int)RentalsConfigrations.RentalDuration))
                return (errorMessage: Errors.InActiveSubscriber, maxAllowedCopies: null);

            var currentRentals = subscriber.Rentals
                .Where(r => rentalId == null || r.Id != rentalId)
                .SelectMany(r => r.RentalCopies)
                .Count(c => !c.ReturnDate.HasValue);

            var availableCopiesCount = (int)RentalsConfigrations.MaxAllowedCopies - currentRentals;

            if (availableCopiesCount.Equals(0))
                return (errorMessage: Errors.MaxCopiesReached, maxAllowedCopies: null);

            return (errorMessage: string.Empty, maxAllowedCopies: availableCopiesCount);
        }




    }
}
