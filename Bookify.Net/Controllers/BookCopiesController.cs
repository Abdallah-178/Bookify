

namespace Bookify.Net.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BookCopiesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public BookCopiesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }

        [AjaxOnly]
        public IActionResult Create(int BookId)
        {
            var book = _context.Books.Find(BookId);

            if (book is null)
                return NotFound();

            var ViewModel = new BookCopyFormViewModel
            {
                BookId = BookId,
                ShowRentalOption = book.IsAvilableForRentel
            };

            return PartialView("Form", ViewModel);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var book = _context.Books.Find(model.BookId);
            if (book is null)
                return NotFound();

            var copy = new BookCopy
            {
                EditionNumber = model.EditionNumber,
                IsAvailableForRental = book.IsAvilableForRentel ? model.IsAvailableForRental : false,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value
            };

            book.Copies.Add(copy);
            _context.SaveChanges();


            var viewmodel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewmodel);


        }

        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var copy = _context.BookCopies.Include(a => a.Book).SingleOrDefault(a => a.id == id);

            if (copy is null)
                return NotFound();

            var ViewModel = _mapper.Map<BookCopyFormViewModel>(copy);
            ViewModel.ShowRentalOption = copy.Book.IsAvilableForRentel;

            return PartialView("Form", ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(BookCopyFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var copy = _context.BookCopies.Include(a => a.Book).SingleOrDefault(a => a.id == model.id);
            if (copy is null)
                return NotFound();

            copy.EditionNumber = model.EditionNumber;
            copy.IsAvailableForRental = copy.Book.IsAvilableForRentel && model.IsAvailableForRental;
            copy.LastUpdatedOn = DateTime.Now;
            copy.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            _context.SaveChanges();


            var viewmodel = _mapper.Map<BookCopyViewModel>(copy);
            return PartialView("_BookCopyRow", viewmodel);


        }


    }
}
