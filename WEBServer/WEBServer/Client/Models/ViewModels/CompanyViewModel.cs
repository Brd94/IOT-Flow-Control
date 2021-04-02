using System.ComponentModel.DataAnnotations;
using WEBServer.Shared;

namespace WEBServer.Client.Models.ViewModels
{
    public class CompanyViewModel
    {

        public int IdLocation { get; set; }

        [Required(ErrorMessage= "Il nome è obbligatorio")]
        public string BusinessName { get; set; }

        [Required(ErrorMessage = "L'indirizzo è obbligatorio")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Il codice postale è obbligatorio")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "La città è obbligatoria")]
        public string City { get; set; }
        
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        [Required(ErrorMessage = "L'orario di apertura è obbligatorio"),
        RegularExpression("^[0-2][0-9]:[0-5][0-9]$",ErrorMessage = "Inserire l'orario in un formato valido (HH:mm)")]
        public string Opening { get; set; }
        
        [Required(ErrorMessage = "L'orario di chiusura è obbligatorio"),
        RegularExpression("^[0-2][0-9]:[0-5][0-9]$",ErrorMessage = "Inserire l'orario in un formato valido (HH:mm)")]
        public string Closing { get; set; }

        public static implicit operator Company(CompanyViewModel cvm)
        {
            return new Company(){
                IdLocation = cvm.IdLocation,
                BusinessName = cvm.BusinessName,
                Address = cvm.Address,
                PostalCode = cvm.PostalCode,
                City = cvm.City,
                Latitude = cvm.Latitude,
                Longitude = cvm.Longitude,
                Opening = int.Parse(cvm.Opening.Replace(":","")),
                Closing = int.Parse(cvm.Closing.Replace(":",""))
            };
        }
    }
}