using AutoMapper.QueryableExtensions;
using CloudinaryDotNet;
using Microsoft.Extensions.Options;
using System.Data;
using System.Linq.Dynamic.Core;

namespace Bookify.Net.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class BooksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;


        private readonly Cloudinary _cloudinary;
        public BooksController(ApplicationDbContext context, IMapper mapper, IOptions<CloudinarySettings> cloudinary, IImageService imageService)
        {
            _context = context;
            _mapper = mapper;

            Account account = new()
            {

                Cloud = cloudinary.Value.Cloud,
                ApiKey = cloudinary.Value.ApiKey,
                ApiSecret = cloudinary.Value.ApiSecret,
            };

            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
            _imageService = imageService;

        }


        public IActionResult Index()
        {
            return View();
        }



        [AjaxOnly]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetBooks()
        {
            var draw = int.TryParse(Request.Form["draw"], out var d) ? d : 0;
            var skip = int.TryParse(Request.Form["start"], out var s) && s >= 0 ? s : 0;
            var pageSize = int.TryParse(Request.Form["length"], out var l) && l > 0 ? Math.Min(l, 100) : 10;
            var searchValue = Request.Form["search[value]"].ToString();

            var sortColumnIndex = Request.Form["order[0][column]"].ToString();
            var sortColumn = Request.Form[$"columns[{sortColumnIndex}][name]"].ToString();
            var sortDirection = Request.Form["order[0][dir]"].ToString() == "asc" ? "asc" : "desc";

            var sortableColumns = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Id"] = "Id",
                ["Title"] = "Title",
                ["publisher"] = "Publisher",
                ["PublishingDate"] = "PublishingDate",
                ["hall"] = "Hall",
                ["IsAvilableForRentel"] = "IsAvilableForRentel",
                ["isDeleted"] = "IsDeleted"
            };

            var orderBy = sortableColumns.TryGetValue(sortColumn, out var column) ? column : "Id";

            // Stable ordering: always tie-break by Id so paging is deterministic
            var orderExpression = orderBy.Equals("Id", StringComparison.OrdinalIgnoreCase)
                ? $"Id {sortDirection}"
                : $"{orderBy} {sortDirection}, Id {sortDirection}";

            IQueryable<Book> books = _context.Books.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                books = books.Where(b =>
                    b.Title.Contains(searchValue) ||
                    b.Author!.Name.Contains(searchValue) ||
                    b.Categories.Any(c => c.Category!.Name.Contains(searchValue)));
            }

            var recordsTotal = await _context.Books.CountAsync();
            var recordsFiltered = await books.CountAsync();

            var data = await books
                .OrderBy(orderExpression)
                .Skip(skip)
                .Take(pageSize)
                .ProjectTo<BookViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(new { draw, recordsTotal, recordsFiltered, data });
        }



        public IActionResult Details(int id)
        {
            var book = _context.Books
                .Include(a => a.Author)
                .Include(c => c.Copies)
                .Include(b => b.Categories)
                .ThenInclude(c => c.Category)
                .SingleOrDefault(b => b.id == id);

            if (book is null)
                return NotFound();

            var ViewModel = _mapper.Map<BookViewModel>(book);

            return View(ViewModel);
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", PopulateViewModel());
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _mapper.Map<Book>(model);

            // Begin Save File In Server //
            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName).ToLowerInvariant()}"; // Give Him Guid Name+Extenstion

                var result = await _imageService.UploadAsync(model.Image, imageName, "/assets/images/Books", hasThumbnail: true);
                if (!result.isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    return View("Form", PopulateViewModel(model));

                }
                book.ImageUrl = imageName;
                book.custom_img = imageName;

            }
            // End Save File In Server //
            book.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            foreach (var item in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = item });

            _context.Add(book);
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.id });
        }




        [HttpGet]
        public IActionResult Edit(int id)
        {
            var book = _context.Books.Include(c => c.Categories).SingleOrDefault(b => b.id == id);

            if (book == null)
                return NotFound();


            var ViewModel = _mapper.Map<BookFormViewModel>(book);


            ViewModel.SelectedCategories = book.Categories.Select(c => c.CategoryId).ToList(); // Handel Categories

            return View("Form", PopulateViewModel(ViewModel));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BookFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var book = _context.Books.Include(c => c.Categories).Include(c => c.Copies).SingleOrDefault(b => b.id == model.id);

            if (book is null)
                return NotFound();

            // Begin  Handel File Upload 
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(book.ImageUrl))
                {
                    _imageService.Delete(book.ImageUrl, book.custom_img);

                }
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName).ToLowerInvariant()}"; // Give Him Guid Name+Extenstion

                var result = await _imageService.UploadAsync(model.Image, imageName, "/assets/images/Books", hasThumbnail: true);
                if (!result.isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    return View("Form", PopulateViewModel(model));

                }
                model.ImageUrl = imageName;
                model.custom_img = imageName;
            }
            else if (!string.IsNullOrEmpty(book.ImageUrl))
            {
                model.ImageUrl = book.ImageUrl;   // save
                model.custom_img = book.custom_img;
            }

            // End  Handel File Upload 

            book = _mapper.Map(model, book);
            book.LastUpdatedOn = DateTime.UtcNow;
            book.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            foreach (var item in model.SelectedCategories)
                book.Categories.Add(new BookCategory { CategoryId = item });

            if (!model.IsAvilableForRentel)
            {
                foreach (var copy in book.Copies)
                    copy.IsAvailableForRental = false;

            }
            _context.SaveChanges();

            return RedirectToAction(nameof(Details), new { id = book.id });
        }


        public IActionResult AllowItem(BookFormViewModel model)
        {
            var book = _context.Books.SingleOrDefault(c => c.Title == model.Title && c.AuthorId == model.AuthorId);

            var isAllawed = book is null || book.id.Equals(model.id);

            return Json(isAllawed);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Togglestatus(int id)
        {


            var book = _context.Books.Find(id);

            if (book is null)
                return NotFound();


            book.IsDeleted = !book.IsDeleted;
            book.LastUpdatedOn = DateTime.Now;
            book.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.SaveChanges();


            return Ok(book.LastUpdatedOn.ToString());
        }

        private BookFormViewModel PopulateViewModel(BookFormViewModel? model = null)
        {
            BookFormViewModel viewModel = model is null ? new BookFormViewModel() : model;

            var AuthorList = _context.Authors.Where(a => !a.IsDeleted).OrderByDescending(a => a.Name).ToList();
            var CategoryList = _context.Categories.Where(a => !a.IsDeleted).OrderByDescending(a => a.Name).ToList();


            viewModel.Authors = _mapper.Map<IEnumerable<SelectListItem>>(AuthorList);
            viewModel.Categories = _mapper.Map<IEnumerable<SelectListItem>>(CategoryList);

            return viewModel;

            //model ??= new BookFormViewModel();
            //model.Authors = _context.Authors
            //  .AsNoTracking()
            //  .Where(a => !a.IsDeleted)
            //  .OrderBy(a => a.Name)
            //  .Select(a => new SelectListItem
            //  {
            //      Value = a.id.ToString(),
            //      Text = a.Name
            //  })
            //  .ToList();

            //model.Categories = _context.Categories
            //    .AsNoTracking()
            //    .Where(c => !c.IsDeleted)
            //    .OrderBy(c => c.Name)
            //    .Select(c => new SelectListItem
            //    {
            //        Value = c.Id.ToString(),
            //        Text = c.Name
            //    })
            //    .ToList();

            //return model;

        }

        private string GetThumbnailUrl(string url)
        {
            var separator = "image/upload/";
            var urlparts = url.Split(separator);
            var thumbnailUrl = $"{urlparts[0]}{separator}e_cartoonify:11:0/{urlparts[1]}";

            return thumbnailUrl;
        }



    }
}
