namespace Bookify.Net.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class AuthorsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;



        public AuthorsController(ApplicationDbContext context, IMapper mapper, IConfiguration configuration)
        {
            _context = context;
            _mapper = mapper;


        }



        [HttpGet]
        public IActionResult Index()
        {
            var authors = _context.Authors.AsNoTracking().ToList();

            var viewmodel = _mapper.Map<IEnumerable<AuthorViewModel>>(authors);
            return View(viewmodel);
        }


        [HttpGet]
        [AjaxOnly]
        public IActionResult Create()
        {
            return PartialView("_Form");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();



            var author = _mapper.Map<Author>(model);
            author.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Add(author);
            _context.SaveChanges();

            var viewModel = _mapper.Map<AuthorViewModel>(author);


            return PartialView("_AuthorRow", viewModel);
        }


        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {
            var Author = _context.Authors.Find(id);
            if (Author is null)
                return NotFound();

            var ViewModel = _mapper.Map<AuthorFormViewModel>(Author);

            return PartialView("_Form", ViewModel);
        }


        [HttpPost]
        [AjaxOnly]
        public IActionResult Edit(AuthorFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var Author = _context.Authors.Find(model.id);

            if (Author is null)
                return NotFound();

            Author = _mapper.Map(model, Author);
            Author.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            Author.LastUpdatedOn = DateTime.Now;

            _context.SaveChanges();

            var viewmodel = _mapper.Map<AuthorViewModel>(Author);

            return PartialView("_AuthorRow", viewmodel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Togglestatus(int id)
        {


            var Authors = _context.Authors.Find(id);

            if (Authors is null)
                return NotFound();


            Authors.IsDeleted = !Authors.IsDeleted;
            Authors.LastUpdatedOn = DateTime.Now;
            Authors.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            _context.SaveChanges();



            return Ok(Authors.LastUpdatedOn.ToString());
        }


        public async Task<IActionResult> AllowItem(CategoryFormViewModel model)
        {
            var Authors = _context.Authors.SingleOrDefault(c => c.Name == model.Name);
            var isAllawed = Authors is null || Authors.id.Equals(model.Id);

            return Json(isAllawed);

        }
    }
}

