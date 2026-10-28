using System;

namespace CliniqueVeterinaire.Models
{
    public class Facture
    {
        public int Id { get; set; }
        public DateTime DateEmission { get; set; }
        public decimal MontantTotal { get; set; }
        public string StatutPaiement { get; set; }
        public int ConsultationId { get; set; }
    }
}