namespace Bookify.Net.Tasks
{
    public class HangfireTasks
    {


        private readonly ApplicationDbContext _context;

        private readonly IWhatsAppClient _whatsAppClient;
        private readonly IWebHostEnvironment _webHostEnvironment;


        private readonly IEmailSender _emailSender;
        private readonly IEmailBodyBuilder _emailBodyBuilder;

        public HangfireTasks(ApplicationDbContext context, IWhatsAppClient whatsAppClient, IWebHostEnvironment webHostEnvironment, IEmailSender emailSender, IEmailBodyBuilder emailBodyBuilder)
        {
            _context = context;



            _whatsAppClient = whatsAppClient;
            _webHostEnvironment = webHostEnvironment;
            _emailBodyBuilder = emailBodyBuilder;
            _emailSender = emailSender;
        }


        // Task 1
        public async Task PrepareExpirationAlert()
        {
            var subscribers = _context.Subsecribers
                                .Include(s => s.Subscriptions)
                                .Where(s => !s.IsBlackListed && s.Subscriptions
                                .OrderByDescending(x => x.EndDate)
                                .First().EndDate == DateTime.Today.AddDays(5))
                                .ToList();

            foreach (var subscriber in subscribers)
            {
                var EndDate = subscriber.Subscriptions.Last().EndDate.ToString("d MMM,yyyy");
                //Send email and WhatsApp Message

                var placeholders = new Dictionary<string, string>()
                    {
                     { "imageUrl", "https://res.cloudinary.com/devcreed/image/upload/v1668739431/icon-positive-vote-2_jcxdww.svg" },
                     { "header", $"Hello {subscriber.FirstName}," },
                     { "body", $"Your Subscriptions Will be Expired by {EndDate} " },
                    };

                var body = _emailBodyBuilder.GetEmailBody(EmailTemplates.Notification, placeholders);

                BackgroundJob.Schedule(() =>
                     _emailSender.SendEmailAsync(
                     subscriber.Email,
                    "Bookify Subsecription Expiration", body), TimeSpan.FromMinutes(1));


                // End Send Wellcome Email

                // Begin Send Whatsapp
                if (subscriber.HasWhatsApp)
                {
                    var components = new List<WhatsAppComponent>
                {
                    new WhatsAppComponent
                            {
                                Type = "body",
                                Parameters = new List<object>
                                {
                                    new WhatsAppTextParameter { Text = subscriber.FirstName },
                                    new WhatsAppTextParameter { Text = EndDate }
                                }
                            }
                };


                    var numbers = PhoneHelper.ToInternationalPalestine(subscriber.MobileNumber);
                    // var mobileNumber = _webHostEnvironment.IsDevelopment() ? "+972595379272" : subscriber.MobileNumber;


                    var result = await _whatsAppClient.SendMessage(
                    numbers.Primary,
                    WhatsAppLanguageCode.English,
                    WhatsappTemplates.SubscriperRenew,   // Must changed
                    components
                );

                    bool success = result?.Error == null && (result?.Messages?.Any() ?? false);

                    if (!success)
                    {
                        BackgroundJob.Enqueue(() =>

                           _whatsAppClient.SendMessage(
                            numbers.Fallback,
                            WhatsAppLanguageCode.English,
                            WhatsappTemplates.appointment_reminder,
                            components
                        ));



                    }

                }
                // End Send Whatsapp
            }

        }


    }
}
