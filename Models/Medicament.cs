using System;

namespace CliniqueVeterinaire.Models
{
    public class Medicament
    {
        public int Id { get; set; }
        public string NomCommercial { get; set; }
        public int QuantiteStock { get; set; }
        public DateTime DateExpiration { get; set; }

        public bool EstExpire => DateExpiration < DateTime.Now;
        public bool StockFaible => QuantiteStock < 10;
    }
}