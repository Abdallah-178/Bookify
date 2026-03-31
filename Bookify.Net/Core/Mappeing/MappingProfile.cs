

namespace Bookify.Net.Core.Mappeing
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category
            CreateMap<Category, CategoryViewModel>();
            CreateMap<CategoryFormViewModel, Category>().ReverseMap();
            CreateMap<Category, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));


            //Authors
            CreateMap<Author, AuthorViewModel>();
            CreateMap<AuthorFormViewModel, Author>().ReverseMap();
            CreateMap<Author, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.id))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));


            // Books
            CreateMap<Book, BookFormViewModel>()
                 .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ReverseMap();

            CreateMap<Book, BookViewModel>()
               .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author!.Name))
               .ForMember(dest => dest.Categories, opt => opt.MapFrom(src => src.Categories.Select(c => c.Category!.Name).ToList()));// مهمه جدا


            // BooksCopy
            CreateMap<BookCopy, BookCopyViewModel>()
                .ForMember(dest => dest.BookTitle, opt => opt.MapFrom(src => src.Book.Title))
                .ForMember(dest => dest.BookCustom_img, opt => opt.MapFrom(src => src.Book.custom_img))
                .ForMember(dest => dest.BookId, opt => opt.MapFrom(src => src.Book.id));

            CreateMap<BookCopy, BookCopyFormViewModel>();


            // Users
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<UserFormViewModel, ApplicationUser>()
                 .ForMember(dest => dest.NormalizedEmail, opt => opt.MapFrom(src => src.Email.ToUpper()))
                 .ForMember(dest => dest.NormalizedUserName, opt => opt.MapFrom(src => src.UserName.ToUpper()))
                .ReverseMap();

            //Subsecribers
            CreateMap<Subsecriber, SubscriberFormViewModel>()
                .ReverseMap();

            CreateMap<Subsecriber, SubscriberViewModel>()
                         .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                         .ForMember(dest => dest.Area, opt => opt.MapFrom(src => src.Area!.Name))
                         .ForMember(dest => dest.Governorate, opt => opt.MapFrom(src => src.Governorate!.Name));

            CreateMap<Subsecriber, SubscriberSearchResultaViewModel>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));



            //Governorate Select
            CreateMap<Governorate, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString())).
                ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));

            //Area Select
            CreateMap<Area, SelectListItem>()
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Id.ToString())).
                ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Name));


            //Subscription
            CreateMap<Subscription, SubscriptionViewModel>();


            // Rentals
            CreateMap<Rental, RentalViewModel>();
            CreateMap<RentalCopy, RentalCopyViewModel>();



        }
    }
}
