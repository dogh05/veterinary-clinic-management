using System;

namespace CliniqueVeterinaire.Models
{
    public class Animal
    {
        public int Id { get; set; }
        public string Nom { get; set; }
        public string Espece { get; set; }
        public string Race { get; set; }
        public DateTime DateNaissance { get; set; }
        public int ProprietaireId { get; set; }
    }
}