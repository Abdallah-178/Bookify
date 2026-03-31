using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Encodings.Web;

namespace Bookify.Net.Controllers
{
    [Authorize(Roles = AppRoles.Admin)]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailSender _emailsender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public UsersController(UserManager<ApplicationUser> userManager, IMapper mapper, RoleManager<IdentityRole> roleManager, IEmailSender emailsender, IEmailBodyBuilder emailBodyBuilder)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _emailsender = emailsender;
            _emailBodyBuilder = emailBodyBuilder;
        }

        public async Task<IActionResult> Index()
        {


            var user = await _userManager.Users.ToListAsync();

            var ViewModel = _mapper.Map<IEnumerable<UserViewModel>>(user);

            return View(ViewModel);
        }

        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Create()
        {
            var ViewModel = new UserFormViewModel
            {
                Roles = await _roleManager.Roles.
                                  Select(r => new SelectListItem
                                  {
                                      Text = r.Name,
                                      Value = r.Name

                                  })
                                  .ToListAsync()
            };

            return PartialView("_Form", ViewModel);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage);

                return BadRequest();
            }

            var user = new ApplicationUser
            {
                FullName = model.FullName,
                UserName = model.UserName,
                Email = model.Email,
                CreatedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value,

            };



            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRolesAsync(user, model.SelectedRoles);

                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = user.Id, code = code },
                    protocol: Request.Scheme);

                var placeholders = new Dictionary<string, string>()
            {
                { "imageUrl",  "https://res.cloudinary.com/devcreed/image/upload/v1668732314/icon-positive-vote-1_rdexez.svg" },
                { "header",   $"Hey {user.FullName}, thanks for joining us!"},
                { "body", "please confirm your email" },
                { "url", $"{HtmlEncoder.Default.Encode(callbackUrl!)}" },
                { "linkTitle", "Activate Account!" }
            };

                var body = _emailBodyBuilder.GetEmailBody(
                       EmailTemplates.Email, placeholders
                    );

                await _emailsender.SendEmailAsync(user.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");



                var viewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UsersRow", viewModel);
            }

            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }






        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> Edit(string id)
        {

            var user = await _userManager.FindByIdAsync(id);

            if (user is null)
                return NotFound();

            var ViewModel = _mapper.Map<UserFormViewModel>(user);
            ViewModel.SelectedRoles = await _userManager.GetRolesAsync(user);
            ViewModel.Roles = await _roleManager.Roles.
                                  Select(r => new SelectListItem
                                  {
                                      Text = r.Name,
                                      Value = r.Name

                                  })
                                  .ToListAsync();



            return PartialView("_Form", ViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserFormViewModel model)
        {

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            user = _mapper.Map(model, user);
            user.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.Now;

            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                var currentRoles = await _userManager.GetRolesAsync(user);

                var rolesUpdated = !currentRoles.SequenceEqual(model.SelectedRoles);

                if (rolesUpdated)
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRolesAsync(user, model.SelectedRoles);
                }

                await _userManager.UpdateSecurityStampAsync(user);

                var ViewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UsersRow", ViewModel);
            }



            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }






        [HttpGet]
        [AjaxOnly]
        public async Task<IActionResult> ResetPassword(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound();
            var ViewModel = new ResetPasswordFormViewModel
            {
                Id = user.Id
            };
            return PartialView("_ResetPasswordForm", ViewModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordFormViewModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user is null)
                return NotFound();

            var currentPasswordHash = user.PasswordHash;

            await _userManager.RemovePasswordAsync(user);

            var result = await _userManager.AddPasswordAsync(user, model.Password);

            if (result.Succeeded)
            {
                user.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
                user.LastUpdatedOn = DateTime.Now;

                await _userManager.UpdateAsync(user);

                var ViewModel = _mapper.Map<UserViewModel>(user);
                return PartialView("_UsersRow", ViewModel);
            }
            user.PasswordHash = currentPasswordHash;
            await _userManager.UpdateAsync(user);
            return BadRequest(string.Join(',', result.Errors.Select(e => e.Description)));
        }







        public async Task<IActionResult> AllowItemUserName(UserFormViewModel model)
        {
            var Users = await _userManager.FindByNameAsync(model.UserName);
            var isAllawed = Users is null || Users.Id.Equals(model.Id);

            return Json(isAllawed);

        }
        public async Task<IActionResult> AllowItemEmail(UserFormViewModel model)
        {
            var Users = await _userManager.FindByEmailAsync(model.Email);
            var isAllawed = Users is null || Users.Id.Equals(model.Id);

            return Json(isAllawed);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            user.IsDeleted = !user.IsDeleted;
            user.LastUpdateedById = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            user.LastUpdatedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            if (user.IsDeleted)
                await _userManager.UpdateSecurityStampAsync(user);

            return Ok(user.LastUpdatedOn.ToString());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnLockUser(string id)
        {

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound();

            var isLocked = await _userManager.IsLockedOutAsync(user);

            if (isLocked)
                await _userManager.SetLockoutEndDateAsync(user, null);


            return Ok();
        }




    }
}
