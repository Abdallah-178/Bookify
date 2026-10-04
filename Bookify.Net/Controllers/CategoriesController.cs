namespace Bookify.Net.Controllers
{
    [Authorize(Roles = AppRoles.Archive)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;


        public CategoriesController(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public IActionResult Index()
        {

            var category = _context.Categories.AsNoTracking().ToList();

            var viewmodel = _mapper.Map<IEnumerable<CategoryViewModel>>(category);

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
        public IActionResult Create(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _mapper.Map<Category>(model);

            category.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            _context.Categories.Add(category);
            _context.SaveChanges();

            var viewmodel = _mapper.Map<CategoryViewModel>(category);

            return PartialView("_CategoryRow", viewmodel);

        }

        [HttpGet]
        [AjaxOnly]
        public IActionResult Edit(int id)
        {

            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();

            var viewmodel = _mapper.Map<CategoryFormViewModel>(category);
            return PartialView("_Form", viewmodel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(CategoryFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var category = _context.Categories.Find(model.Id);

            if (category is null)
                return NotFound();


            category = _mapper.Map(model, category);

            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;



            _context.SaveChanges();


            _context.SaveChanges();
            var viewmodel = _mapper.Map<CategoryViewModel>(category);


            return PartialView("_CategoryRow", viewmodel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Togglestatus(int id)
        {


            var category = _context.Categories.Find(id);

            if (category is null)
                return NotFound();


            category.IsDeleted = !category.IsDeleted;
            category.LastUpdatedOn = DateTime.Now;
            category.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;


            _context.SaveChanges();

            return Ok(category.LastUpdatedOn.ToString());
        }


        public async Task<IActionResult> AllowItem(CategoryFormViewModel model)
        {
            var category = _context.Categories.SingleOrDefault(c => c.Name == model.Name);
            var isAllawed = category is null || category.Id.Equals(model.Id);

            return Json(isAllawed);

        }


    }
}
