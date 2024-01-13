namespace Mamba.Areas.Admin.ViewModels
{
    public class UpdateServiceVM
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
