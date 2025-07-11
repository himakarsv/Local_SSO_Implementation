using System.ComponentModel.DataAnnotations;

namespace SSO.DTO
{
    public class ValidateSSOTokenRequesDTO
    {
        [Required(ErrorMessage = "SSOToken is Required")]
        public string SSOToken { get; set; } = null!;
    }
}
