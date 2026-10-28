using System;

namespace CliniqueVeterinaire.Models
{
    public class Consultation
    {
        public int Id { get; set; }
        public DateTime DateHeure { get; set; }
        public string Diagnostic { get; set; }
        public string Traitement { get; set; }
        public string Remarques { get; set; }
        public int RendezVousId { get; set; }
        public int AnimalId { get; set; }
        public string NomAnimal { get; set; }
    }
}