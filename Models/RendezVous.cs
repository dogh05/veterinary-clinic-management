using System;

namespace CliniqueVeterinaire.Models
{
    public class RendezVous
    {
        public int Id { get; set; }
        public DateTime DateHeure { get; set; }
        public string Motif { get; set; }
        public string Statut { get; set; }
        public int AnimalId { get; set; }
        public int VeterinaireId { get; set; }
    }
}