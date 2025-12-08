    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;
    using Systeme_RH.Models;
    
    public class ApplicationDbContext 
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Employe>().ToTable("Employe");
            builder.Entity<Poste>().ToTable("Poste");
            builder.Entity<Departement>().ToTable("Departement");
            builder.Entity<Contrat>().ToTable("Contrat");
            builder.Entity<DemandeConge>().ToTable("DemandeConge");
            builder.Entity<Conge>().ToTable("Conge");

            // Configuration de la relation 1-1 entre ApplicationUser et Employe
            builder.Entity<Employe>()
                .HasOne(e => e.ApplicationUser)      // Un Employé a un User
                .WithOne(u => u.Employe)             // Un User a un Employé
                .HasForeignKey<Employe>(e => e.ApplicationUserId) // C'est Employé qui porte la clé !
                .OnDelete(DeleteBehavior.Restrict);   // Si on supprime le User, on supprime l'Employé (ou SetNull selon ton choix)
            
            builder.Entity<Employe>()
                .HasOne(e => e.Departement)
                .WithMany(d => d.Employes)
                .HasForeignKey(e => e.DepartementId)
                .OnDelete(DeleteBehavior.Restrict);
                
            // Configuration des types décimaux pour éviter les warnings
            builder.Entity<Employe>()
                .Property(e => e.SoldeConges)
                .HasColumnType("decimal(18,2)");

            builder.Entity<Poste>()
                .Property(p => p.Salaire).HasColumnType("decimal(18,2)");
                
            builder.Entity<Contrat>()
                .Property(c => c.SalaireMensuel).HasColumnType("decimal(18,2)");
        }

        public DbSet<Employe> Employes { get; set; }
        public DbSet<Departement> Departements { get; set; }
        public DbSet<Poste> Postes { get; set; }
        public DbSet<Contrat> Contrats { get; set; }
        public DbSet<Conge> Conges { get; set; }
        public DbSet<DemandeConge> DemandeConges { get; set; }
    }
