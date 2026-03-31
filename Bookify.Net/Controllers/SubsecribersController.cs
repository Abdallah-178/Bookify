using Bookify.Net.Helpers;
using Hangfire;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text.Encodings.Web;
using WhatsAppCloudApi;
using WhatsAppCloudApi.Services;

namespace Bookify.Net.Controllers
{
    public class SubsecribersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IImageService _imageService;
        private readonly IDataProtector _dataProtector;

        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;


        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;


        public SubsecribersController(ApplicationDbContext context, IMapper mapper, IImageService imageService, IDataProtectionProvider dataProtector, IWhatsAppClient whatsAppClient, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _context = context;
            _mapper = mapper;
            _imageService = imageService;
            _dataProtector = dataProtector.CreateProtector("MySecureKey");
            _whatsAppClient = whatsAppClient;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }


        public async Task<IActionResult> Index()
        {
            return View();
        }


        [AjaxOnly]
        public IActionResult GetAreas(int governoratesId)
        {
            var area = _context.Areas
                .Where(a => a.Governorateid == governoratesId)
               .Select(a => new SelectListItem
               {
                   Value = a.Id.ToString(),
                   Text = a.Name
               })
                  .OrderByDescending(a => a.Text)
                  .ToList();
            var ViewModel = _mapper.Map<IEnumerable<SelectListItem>>(area);
            return Ok(ViewModel);
        }



        public IActionResult Create()
        {
            var ViewModel = new SubscriberFormViewModel
            {
                Governorates = _context.Governorates
                              .Where(a => !a.IsDeleted)
                              .Select(a => new SelectListItem
                              {
                                  Value = a.Id.ToString(),
                                  Text = a.Name
                              })
                              .OrderByDescending(a => a.Text)
                               .ToList()
            };
            return View("Form", ViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var Subscribers = _mapper.Map<Subsecriber>(model);

            // Begin Save File In Server //
            if (model.Image is not null)
            {
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}"; // Give Him Guid Name+Extenstion

                var result = await _imageService.UploadAsync(model.Image, imageName, "/assets/images/Subscribers", hasThumbnail: true);
                if (!result.isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    return View("Form", PopulateViewModel(model));

                }
                Subscribers.ImageUrl = imageName;
                Subscribers.custom_img = imageName;


            }
            // End Save File In Server //
            Subscribers.CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

            Subscription Subscription = new()
            {
                Subsecriberid = Subscribers.Id,
                CreatedById = Subscribers.CreatedById,
                CreatedOn = Subscribers.CreatedOn,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddYears(1)

            };

            Subscribers.Subscriptions.Add(Subscription);
            _context.Add(Subscribers);

            _context.SaveChanges();

            // Begin Send Wellcome Email


            var placeholders = new Dictionary<string, string>()
                    {
                     { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1668739431/icon-positive-vote-2_jcxdww.svg" },
                     { "header", $"Welcome {model.FirstName}," },
                     { "body", "Thanks For Joining In Bookify WebSite😍😍📖📖📚📖📚\r\n🐓🐓🎉🎉🎉🎊🎊🎊" },
                    };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() => 
            _emailSender.SendEmailAsync(
                model.Email!,
                "Welcom To Bookify",
                body));

            // End Send Wellcome Email

            // End Send Wellcome Email

            if (model.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>
                {
                    new WhatsAppComponent
                            {
                                Type = "body",
                                Parameters = new List<object>
                                {
                                    new WhatsAppTextParameter { Text = model.FirstName }
                                }
                            }
                };

                var numbers = PhoneHelper.ToInternationalPalestine(model.MobileNumber);

                var result = await _whatsAppClient.SendMessage(
                    numbers.Primary,
                    WhatsAppLanguageCode.English,
                   WhatsappTemplates.WelcomeMessage,
                    components
                );

                bool success = result?.Error == null && (result?.Messages?.Any() ?? false);

                if (!success)
                {
                    BackgroundJob.Enqueue(() =>

                       _whatsAppClient.SendMessage(
                        numbers.Fallback,
                        WhatsAppLanguageCode.English,
                        WhatsappTemplates.WelcomeMessage,
                        components
                    ));

                   

                }

            }

            // End Send Whatsapp

            var SubscriberId = _dataProtector.Protect(Subscribers.Id.ToString());

            return RedirectToAction(nameof(Details), new { id = SubscriberId });
        }


        [HttpGet]
        public IActionResult Edit(string id)
        {
            var SubscriberId = int.Parse(_dataProtector.Unprotect(id));
            var Subsecribers = _context.Subsecribers.Find(SubscriberId);


            if (Subsecribers == null)
                return NotFound();


            var ViewModel = _mapper.Map<SubscriberFormViewModel>(Subsecribers);
            ViewModel.Key = id;

            return View("Form", PopulateViewModel(ViewModel));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SubscriberFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View("Form", PopulateViewModel(model));

            var SubscriberId = int.Parse(_dataProtector.Unprotect(model.Key));

            var Subscriber = _context.Subsecribers.SingleOrDefault(b => b.Id == SubscriberId);

            if (Subscriber is null)
                return NotFound();

            // Begin  Handel File Upload 
            if (model.Image is not null)
            {
                if (!string.IsNullOrEmpty(Subscriber.ImageUrl))
                {
                    _imageService.Delete(Subscriber.ImageUrl, Subscriber.custom_img);

                }
                var imageName = $"{Guid.NewGuid()}{Path.GetExtension(model.Image.FileName)}"; // Give Him Guid Name+Extenstion

                var result = await _imageService.UploadAsync(model.Image, imageName, "/assets/images/Subscribers", hasThumbnail: true);
                if (!result.isUploaded)
                {
                    ModelState.AddModelError(nameof(Image), result.errorMessage!);
                    return View("Form", PopulateViewModel(model));

                }
                model.ImageUrl = imageName;
                model.custom_img = imageName;


            }
            else if (!string.IsNullOrEmpty(Subscriber.ImageUrl))
            {
                model.ImageUrl = Subscriber.ImageUrl;   // save
                model.custom_img = Subscriber.custom_img;
            }

            // End  Handel File Upload 

            Subscriber = _mapper.Map(model, Subscriber);
            Subscriber.LastUpdatedOn = DateTime.UtcNow;
            Subscriber.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;




            _context.SaveChanges();


            var SubscriberID = _dataProtector.Protect(Subscriber.Id.ToString());
            return RedirectToAction(nameof(Details), new { id = SubscriberID });
        }


        public IActionResult Details(string id)
        {
            var SubscriberId = int.Parse(_dataProtector.Unprotect(id));


            var Subscriber = _context.Subsecribers
                .Include(a => a.Area)
                .Include(a => a.Governorate)
                .Include(a => a.Subscriptions)
                .Include(a => a.Rentals)
                .ThenInclude(a => a.RentalCopies)
                .SingleOrDefault(s => s.Id == SubscriberId);


            if (Subscriber is null)
                return NotFound();

            var ViewModel = _mapper.Map<SubscriberViewModel>(Subscriber);
            ViewModel.Key = id;


            return View(ViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(SearchFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var Subscriber = _context.Subsecribers.SingleOrDefault(s => s.Email == model.Value ||
                                                                  s.MobileNumber == model.Value ||
                                                                   s.NationalId == model.Value);

            var ViewModel = _mapper.Map<SubscriberSearchResultaViewModel>(Subscriber);

            if (Subscriber is not null)
                ViewModel.Key = _dataProtector.Protect(Subscriber.Id.ToString());

            return PartialView("_Result", ViewModel);


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RenewSubscriptionAsync(int sKey)
        {
            var subscriber = _context.Subsecribers
                                        .Include(s => s.Subscriptions)
                                        .SingleOrDefault(s => s.Id == sKey);

            if (subscriber is null)
                return NotFound();

            if (subscriber.IsBlackListed)
                return BadRequest();

            var lastSubscription = subscriber.Subscriptions.Last();

            var startDate = lastSubscription.EndDate < DateTime.Today
                            ? DateTime.Today
                            : lastSubscription.EndDate.AddDays(1);

            Subscription newSubscription = new()
            {
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,
                CreatedOn = DateTime.Now,
                StartDate = startDate,
                EndDate = startDate.AddYears(1)
            };

            subscriber.Subscriptions.Add(newSubscription);

            _context.SaveChanges();

            //Send email and WhatsApp Message


            var placeholders = new Dictionary<string, string>()
                    {
                     { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1668739431/icon-positive-vote-2_jcxdww.svg" },
                     { "header", $"Hello {subscriber.FirstName}," },
                     { "body", $"Your Subsecription has been Renewed through {newSubscription.EndDate.ToString()}" },
                    };

            var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

            BackgroundJob.Enqueue(() =>
            _emailSender.SendEmailAsync(
                subscriber.Email,
                "Bookify Subsecription Renewel", body));

            // End Send Wellcome Email

            if (subscriber.HasWhatsApp)
            {
                var components = new List<WhatsAppComponent>
                {
                    new WhatsAppComponent
                            {
                                Type = "body",
                                Parameters = new List<object>
                                {
                                    new WhatsAppTextParameter { Text = subscriber.FirstName }
                                }
                            }
                };

                var numbers = PhoneHelper.ToInternationalPalestine(subscriber.MobileNumber);

                var result = await _whatsAppClient.SendMessage(
                    numbers.Primary,
                    WhatsAppLanguageCode.English,
                   WhatsappTemplates.WelcomeMessage,
                    components
                );

                bool success = result?.Error == null && (result?.Messages?.Any() ?? false);

                if (!success)
                {
                    BackgroundJob.Enqueue(() =>

                       _whatsAppClient.SendMessage(
                        numbers.Fallback,
                        WhatsAppLanguageCode.English,
                        WhatsappTemplates.WelcomeMessage,
                        components
                    ));



                }

            }

            // End Send Whatsapp


            var viewModel = _mapper.Map<SubscriptionViewModel>(newSubscription);

            return PartialView("_SubscriptionRow", viewModel);
        }

        private SubscriberFormViewModel PopulateViewModel(SubscriberFormViewModel? model = null)
        {
            SubscriberFormViewModel viewModel = model is null ? new SubscriberFormViewModel() : model;

            var areas = _context.Areas.Where(a => !a.IsDeleted).OrderByDescending(a => a.Name).ToList();
            var governorates = _context.Governorates.Where(g => !g.IsDeleted).OrderByDescending(g => g.Name).ToList();

            viewModel.Areas = _mapper.Map<IEnumerable<SelectListItem>>(areas);
            viewModel.Governorates = _mapper.Map<IEnumerable<SelectListItem>>(governorates);

            return viewModel;
        }

        public IActionResult AllowItemEmail(SubscriberFormViewModel model)
        {
            var Subscriberid = 0;

            if (!string.IsNullOrEmpty(model.Key))
                Subscriberid = int.Parse(_dataProtector.Unprotect(model.Key));

            var Email = _context.Subsecribers.SingleOrDefault(c => c.Email == model.Email);
            var isAllawed = Email is null || Email.Id.Equals(Subscriberid);

            return Json(isAllawed);

        }
        public IActionResult AllowItemNationalId(SubscriberFormViewModel model)
        {

            var Subscriberid = 0;

            if (!string.IsNullOrEmpty(model.Key))
                Subscriberid = int.Parse(_dataProtector.Unprotect(model.Key));

            var NationalId = _context.Subsecribers.SingleOrDefault(c => c.NationalId == model.NationalId);
            var isAllawed = NationalId is null || NationalId.Id.Equals(Subscriberid);

            return Json(isAllawed);

        }
        public IActionResult AllowItemMobileNumber(SubscriberFormViewModel model)
        {

            var Subscriberid = 0;

            if (!string.IsNullOrEmpty(model.Key))
                Subscriberid = int.Parse(_dataProtector.Unprotect(model.Key));

            var MobileNumber = _context.Subsecribers.SingleOrDefault(c => c.MobileNumber == model.MobileNumber);
            var isAllawed = MobileNumber is null || MobileNumber.Id.Equals(Subscriberid);

            return Json(isAllawed);

        }


    }
}


