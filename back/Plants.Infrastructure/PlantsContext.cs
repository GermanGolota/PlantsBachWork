using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Plants.Core.Entities;

#nullable disable

namespace Plants.Infrastructure
{
    public partial class PlantsContext : DbContext
    {
        public PlantsContext()
        {
        }

        public PlantsContext(DbContextOptions<PlantsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<CurrentUserAddress> CurrentUserAddresses { get; set; }
        public virtual DbSet<CurrentUserRole> CurrentUserRoles { get; set; }
        public virtual DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
        public virtual DbSet<DictsV> DictsVs { get; set; }
        public virtual DbSet<Person> People { get; set; }
        public virtual DbSet<PersonAddressesV> PersonAddressesVs { get; set; }
        public virtual DbSet<PersonCredsV> PersonCredsVs { get; set; }
        public virtual DbSet<PersonToLogin> PersonToLogins { get; set; }
        public virtual DbSet<Plant> Plants { get; set; }
        public virtual DbSet<PlantCaringInstruction> PlantCaringInstructions { get; set; }
        public virtual DbSet<PlantGroup> PlantGroups { get; set; }
        public virtual DbSet<PlantOrder> PlantOrders { get; set; }
        public virtual DbSet<PlantPost> PlantPosts { get; set; }
        public virtual DbSet<PlantPostV> PlantPostVs { get; set; }
        public virtual DbSet<PlantRegion> PlantRegions { get; set; }
        public virtual DbSet<PlantSearchV> PlantSearchVs { get; set; }
        public virtual DbSet<PlantShipment> PlantShipments { get; set; }
        public virtual DbSet<PlantSoil> PlantSoils { get; set; }
        public virtual DbSet<PlantStatsV> PlantStatsVs { get; set; }
        public virtual DbSet<PlantToImage> PlantToImages { get; set; }
        public virtual DbSet<PlantToRegion> PlantToRegions { get; set; }
        public virtual DbSet<PlantsV> PlantsVs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Name=ConnectionStrings:Plants");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasPostgresEnum(null, "userroles", new[] { "consumer", "producer", "manager", "other" })
                .HasAnnotation("Relational:Collation", "Russian_Russia.1251");

            modelBuilder.Entity<CurrentUserAddress>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("current_user_addresses");

                entity.Property(e => e.Cities).HasColumnName("cities");

                entity.Property(e => e.Posts).HasColumnName("posts");
            });

            modelBuilder.Entity<CurrentUserRole>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("current_user_roles");
            });

            modelBuilder.Entity<DeliveryAddress>(entity =>
            {
                entity.ToTable("delivery_address");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.City)
                    .IsRequired()
                    .HasColumnName("city");

                entity.Property(e => e.NovaPoshtaNumber).HasColumnName("nova_poshta_number");

                entity.Property(e => e.PersonId).HasColumnName("person_id");

                entity.HasOne(d => d.Person)
                    .WithMany(p => p.DeliveryAddresses)
                    .HasForeignKey(d => d.PersonId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("delivery_address_person_id_fkey");
            });

            modelBuilder.Entity<DictsV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("dicts_v");

                entity.Property(e => e.Ids).HasColumnName("ids");

                entity.Property(e => e.Type).HasColumnName("type");

                entity.Property(e => e.Values).HasColumnName("values");
            });

            modelBuilder.Entity<Person>(entity =>
            {
                entity.ToTable("person");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.FirstName)
                    .IsRequired()
                    .HasColumnName("first_name");

                entity.Property(e => e.LastName)
                    .IsRequired()
                    .HasColumnName("last_name");

                entity.Property(e => e.PhoneNumber)
                    .IsRequired()
                    .HasColumnName("phone_number");
            });

            modelBuilder.Entity<PersonAddressesV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("person_addresses_v");

                entity.Property(e => e.Cities).HasColumnName("cities");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Posts).HasColumnName("posts");
            });

            modelBuilder.Entity<PersonCredsV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("person_creds_v");

                entity.Property(e => e.CaredCount).HasColumnName("cared_count");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InstructionsCount).HasColumnName("instructions_count");

                entity.Property(e => e.SoldCount).HasColumnName("sold_count");
            });

            modelBuilder.Entity<PersonToLogin>(entity =>
            {
                entity.HasKey(e => e.PersonId)
                    .HasName("person_to_login_pkey");

                entity.ToTable("person_to_login");

                entity.Property(e => e.PersonId)
                    .ValueGeneratedNever()
                    .HasColumnName("person_id");

                entity.HasOne(d => d.Person)
                    .WithOne(p => p.PersonToLogin)
                    .HasForeignKey<PersonToLogin>(d => d.PersonId)
                    .HasConstraintName("person_to_login_person_id_fkey");
            });

            modelBuilder.Entity<Plant>(entity =>
            {
                entity.ToTable("plant");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.CareTakerId).HasColumnName("care_taker_id");

                entity.Property(e => e.Created)
                    .HasColumnType("date")
                    .HasColumnName("created");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.PlantName)
                    .IsRequired()
                    .HasColumnName("plant_name");

                entity.Property(e => e.SoilId).HasColumnName("soil_id");

                entity.HasOne(d => d.CareTaker)
                    .WithMany(p => p.Plants)
                    .HasForeignKey(d => d.CareTakerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_care_taker_id_fkey");

                entity.HasOne(d => d.Group)
                    .WithMany(p => p.Plants)
                    .HasForeignKey(d => d.GroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_group_id_fkey");

                entity.HasOne(d => d.Soil)
                    .WithMany(p => p.Plants)
                    .HasForeignKey(d => d.SoilId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_soil_id_fkey");
            });

            modelBuilder.Entity<PlantCaringInstruction>(entity =>
            {
                entity.ToTable("plant_caring_instruction");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.InstructionText)
                    .IsRequired()
                    .HasColumnName("instruction_text");

                entity.Property(e => e.PlantGroupId).HasColumnName("plant_group_id");

                entity.Property(e => e.PostedById).HasColumnName("posted_by_id");

                entity.HasOne(d => d.PlantGroup)
                    .WithMany(p => p.PlantCaringInstructions)
                    .HasForeignKey(d => d.PlantGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_caring_instruction_plant_group_id_fkey");

                entity.HasOne(d => d.PostedBy)
                    .WithMany(p => p.PlantCaringInstructions)
                    .HasForeignKey(d => d.PostedById)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_caring_instruction_posted_by_id_fkey");
            });

            modelBuilder.Entity<PlantGroup>(entity =>
            {
                entity.ToTable("plant_group");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.GroupName)
                    .IsRequired()
                    .HasColumnName("group_name");
            });

            modelBuilder.Entity<PlantOrder>(entity =>
            {
                entity.HasKey(e => e.PostId)
                    .HasName("plant_order_pkey");

                entity.ToTable("plant_order");

                entity.Property(e => e.PostId)
                    .ValueGeneratedNever()
                    .HasColumnName("post_id");

                entity.Property(e => e.Created)
                    .HasColumnType("date")
                    .HasColumnName("created")
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.CustomerId).HasColumnName("customer_id");

                entity.Property(e => e.DeliveryAddressId).HasColumnName("delivery_address_id");

                entity.HasOne(d => d.Customer)
                    .WithMany(p => p.PlantOrders)
                    .HasForeignKey(d => d.CustomerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_order_customer_id_fkey");

                entity.HasOne(d => d.DeliveryAddress)
                    .WithMany(p => p.PlantOrders)
                    .HasForeignKey(d => d.DeliveryAddressId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_order_delivery_address_id_fkey");

                entity.HasOne(d => d.Post)
                    .WithOne(p => p.PlantOrder)
                    .HasForeignKey<PlantOrder>(d => d.PostId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_order_post_id_fkey");
            });

            modelBuilder.Entity<PlantPost>(entity =>
            {
                entity.HasKey(e => e.PlantId)
                    .HasName("plant_post_pkey");

                entity.ToTable("plant_post");

                entity.Property(e => e.PlantId)
                    .ValueGeneratedOnAdd()
                    .HasColumnName("plant_id");

                entity.Property(e => e.Created)
                    .HasColumnType("date")
                    .HasColumnName("created")
                    .HasDefaultValueSql("CURRENT_DATE");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.SellerId).HasColumnName("seller_id");

                entity.HasOne(d => d.Plant)
                    .WithOne(p => p.PlantPost)
                    .HasForeignKey<PlantPost>(d => d.PlantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_post_plant_id_fkey");

                entity.HasOne(d => d.Seller)
                    .WithMany(p => p.PlantPosts)
                    .HasForeignKey(d => d.SellerId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_post_seller_id_fkey");
            });

            modelBuilder.Entity<PlantPostV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("plant_post_v");

                entity.Property(e => e.CareTakerCared).HasColumnName("care_taker_cared");

                entity.Property(e => e.CareTakerInstructions).HasColumnName("care_taker_instructions");

                entity.Property(e => e.CareTakerSold).HasColumnName("care_taker_sold");

                entity.Property(e => e.Created)
                    .HasColumnType("date")
                    .HasColumnName("created");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.GroupName).HasColumnName("group_name");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Images).HasColumnName("images");

                entity.Property(e => e.PlantName).HasColumnName("plant_name");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.Regions).HasColumnName("regions");

                entity.Property(e => e.SellerCared).HasColumnName("seller_cared");

                entity.Property(e => e.SellerInstructions).HasColumnName("seller_instructions");

                entity.Property(e => e.SellerName).HasColumnName("seller_name");

                entity.Property(e => e.SellerPhone).HasColumnName("seller_phone");

                entity.Property(e => e.SellerSold).HasColumnName("seller_sold");

                entity.Property(e => e.SoilName).HasColumnName("soil_name");
            });

            modelBuilder.Entity<PlantRegion>(entity =>
            {
                entity.ToTable("plant_region");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.RegionName)
                    .IsRequired()
                    .HasColumnName("region_name");
            });

            modelBuilder.Entity<PlantSearchV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("plant_search_v");

                entity.Property(e => e.Created)
                    .HasColumnType("date")
                    .HasColumnName("created");

                entity.Property(e => e.GroupId).HasColumnName("group_id");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.PlantName).HasColumnName("plant_name");

                entity.Property(e => e.Price).HasColumnName("price");

                entity.Property(e => e.Regions).HasColumnName("regions");

                entity.Property(e => e.SoilId).HasColumnName("soil_id");
            });

            modelBuilder.Entity<PlantShipment>(entity =>
            {
                entity.HasKey(e => e.OrderId)
                    .HasName("plant_shipment_pkey");

                entity.ToTable("plant_shipment");

                entity.Property(e => e.OrderId)
                    .ValueGeneratedNever()
                    .HasColumnName("order_id");

                entity.Property(e => e.Shipped)
                    .HasColumnType("date")
                    .HasColumnName("shipped");

                entity.HasOne(d => d.Order)
                    .WithOne(p => p.PlantShipment)
                    .HasForeignKey<PlantShipment>(d => d.OrderId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_shipment_order_id_fkey");
            });

            modelBuilder.Entity<PlantSoil>(entity =>
            {
                entity.ToTable("plant_soil");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.SoilName)
                    .IsRequired()
                    .HasColumnName("soil_name");
            });

            modelBuilder.Entity<PlantStatsV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("plant_stats_v");

                entity.Property(e => e.GroupName).HasColumnName("group_name");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Income).HasColumnName("income");

                entity.Property(e => e.Instructions).HasColumnName("instructions");

                entity.Property(e => e.PlantsCount).HasColumnName("plants_count");

                entity.Property(e => e.Popularity).HasColumnName("popularity");
            });

            modelBuilder.Entity<PlantToImage>(entity =>
            {
                entity.HasKey(e => e.RelationId)
                    .HasName("plant_to_image_pkey");

                entity.ToTable("plant_to_image");

                entity.Property(e => e.RelationId).HasColumnName("relation_id");

                entity.Property(e => e.Image).HasColumnName("image");

                entity.Property(e => e.PlantId).HasColumnName("plant_id");

                entity.HasOne(d => d.Plant)
                    .WithMany(p => p.PlantToImages)
                    .HasForeignKey(d => d.PlantId)
                    .HasConstraintName("plant_to_image_plant_id_fkey");
            });

            modelBuilder.Entity<PlantToRegion>(entity =>
            {
                entity.ToTable("plant_to_region");

                entity.HasIndex(e => new { e.PlantId, e.PlantRegionId }, "plant_to_region_unique")
                    .IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.PlantId).HasColumnName("plant_id");

                entity.Property(e => e.PlantRegionId).HasColumnName("plant_region_id");

                entity.HasOne(d => d.Plant)
                    .WithMany(p => p.PlantToRegions)
                    .HasForeignKey(d => d.PlantId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_to_region_plant_id_fkey");

                entity.HasOne(d => d.PlantRegion)
                    .WithMany(p => p.PlantToRegions)
                    .HasForeignKey(d => d.PlantRegionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("plant_to_region_plant_region_id_fkey");
            });

            modelBuilder.Entity<PlantsV>(entity =>
            {
                entity.HasNoKey();

                entity.ToTable("plants_v");

                entity.Property(e => e.Description).HasColumnName("description");

                entity.Property(e => e.Id).HasColumnName("id");

                entity.Property(e => e.Ismine).HasColumnName("ismine");

                entity.Property(e => e.PlantName).HasColumnName("plant_name");
            });

            modelBuilder.HasSequence("plantgroupidsequence");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
