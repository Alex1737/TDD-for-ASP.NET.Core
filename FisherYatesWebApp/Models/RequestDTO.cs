using System.ComponentModel.DataAnnotations;
namespace FisherYates.Models
{
    public class ValidRequest
    {
        [Required(ErrorMessage = "Input string is required")]
        [RegularExpression(@"^[A-Za-z0-9]+(-[A-Za-z0-9]+)*$", ErrorMessage = "Input must be dasherized string like 'A-B-C')")]
        public required string input { get; set; }
    }
}