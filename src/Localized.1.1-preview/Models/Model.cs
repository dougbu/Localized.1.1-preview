using System.ComponentModel.DataAnnotations;

namespace Localized._1._1_preview.Models
{
    public class Model
    {
        [Required(ErrorMessage = "{0} is totally, completely required.")]
        public int Id { get; set; }

        [Required(ErrorMessage = "{0} must be longer than 4 (four) characters.")]
        [MinLength(4, ErrorMessage = "{0} must be longer than {1} characters.")]
        public string Name { get; set; }
    }
}
