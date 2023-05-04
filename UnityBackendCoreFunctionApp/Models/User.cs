using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace UnityBackendCoreFunctionApp.Models {
    public class User {
        public string StudentUID { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? ProfilePicture { get; set; }
        public string Name { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string CityOfOrigin { get; set; } = null!;
        public string StateOfOrigin { get; set; } = null!;
        public string CountryOfOrigin { get; set; } = null!;
        public string Nationality { get; set; } = null!;
        public string Department { get; set; } = null!;
        public string School { get; set; } = null!;
    }

}
