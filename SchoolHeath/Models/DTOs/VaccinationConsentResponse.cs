using System.ComponentModel.DataAnnotations;

namespace SchoolHeath.Models.DTOs
{
    /// <summary>
    /// DTO for parent response to vaccination consent
    /// </summary>
    public class VaccinationConsentResponse
    {
        /// <summary>
        /// Whether the parent consents to vaccination
        /// </summary>
        [Required]
        public bool ConsentStatus { get; set; }

        /// <summary>
        /// Optional notes from the parent
        /// </summary>
        [StringLength(255)]
        public string? Notes { get; set; }
    }
}