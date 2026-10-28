using CliniqueVeterinaire.Models;

namespace CliniqueVeterinaire.Helpers
{
    public static class SessionUtilisateur
    {
        public static Utilisateur Current { get; set; }

        public static bool IsAuthenticated => Current != null;

        public static void Logout()
        {
            Current = null;
        }
    }
}