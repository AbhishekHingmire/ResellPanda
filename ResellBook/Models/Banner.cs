namespace ResellBook.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Banner
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, StringLength(200)]
        public string Title { get; set; }

        [Display(Name = "Image URL")]
        public string ImageURL { get; set; }

        [Display(Name = "Redirect URL")]
        [DataType(DataType.Url)]
        public string RedirectURL { get; set; }

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Radius { get; set; }
    }

}
