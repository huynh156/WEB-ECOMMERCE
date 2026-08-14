using FashionHubWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace FashionHubWeb
{
    public static class DataSeeder
    {
        public static void Initialize(FashionHubContext context)
        {
            context.Database.EnsureCreated();

            // Seed or update Admin user with full access
            var adminUser = context.Users.FirstOrDefault(u => u.Username == "admin");
            if (adminUser == null)
            {
                adminUser = new User
                {
                    UserId = "ADMIN_001",
                    Username = "admin",
                    Password = "123123",
                    Email = "admin@fashionhub.com",
                    FullName = "System Administrator",
                    Address = "FashionHub HQ",
                    PhoneNumber = 999888777,
                    Role = "Admin",
                    RandomKey = Guid.NewGuid().ToString(),
                    IsActive = true
                };
                context.Users.Add(adminUser);
            }
            else
            {
                adminUser.Password = "123123";
                adminUser.Role = "Admin";
                adminUser.IsActive = true;
            }

            // Seed or update regular Customer user
            var customerUser = context.Users.FirstOrDefault(u => u.Username == "user");
            if (customerUser == null)
            {
                customerUser = new User
                {
                    UserId = "USER_001",
                    Username = "user",
                    Password = "123123",
                    Email = "user@fashionhub.com",
                    FullName = "Sample Customer",
                    Address = "123 Fashion Street",
                    PhoneNumber = 123456789,
                    Role = "Customer",
                    RandomKey = Guid.NewGuid().ToString(),
                    IsActive = true
                };
                context.Users.Add(customerUser);
            }

            context.SaveChanges();

            // Check if old seed data exists and clear it using raw SQL to bypass EF Core tracking and FK conflicts
            if (context.Brands.Any(b => b.BrandName == "Nike" || b.BrandName == "Zara") || !context.Coupons.Any())
            {
                // Delete in correct order of dependency
                context.Database.ExecuteSqlRaw("DELETE FROM [CategoryPromotions]");
                context.Database.ExecuteSqlRaw("DELETE FROM [ProductPromotions]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Reviews]");
                context.Database.ExecuteSqlRaw("DELETE FROM [PurchaseOrderDetails]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Carts]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Wishlist]");
                context.Database.ExecuteSqlRaw("DELETE FROM [OrderDetails]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Orders]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Products]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Categories]");
                context.Database.ExecuteSqlRaw("DELETE FROM [Brands]");
            }

            if (context.Products.Any())
            {
                return;   // DB has been seeded
            }

            var category1 = new Category { CategoryId = "CAT_BAG", CategoryName = "Handbags" };
            var category2 = new Category { CategoryId = "CAT_RTW", CategoryName = "Ready-to-Wear" };
            var category3 = new Category { CategoryId = "CAT_SHOE", CategoryName = "Shoes" };
            var category4 = new Category { CategoryId = "CAT_ACC", CategoryName = "Accessories" };

            context.Categories.AddRange(category1, category2, category3, category4);

            var brand1 = new Brand { BrandId = "BR_LV", BrandName = "Louis Vuitton", Description = "French luxury fashion house founded in 1854." };
            var brand2 = new Brand { BrandId = "BR_DIOR", BrandName = "Dior", Description = "French luxury fashion house founded in 1946 by Christian Dior." };
            var brand3 = new Brand { BrandId = "BR_CHANEL", BrandName = "Chanel", Description = "French luxury fashion house founded in 1910 by Coco Chanel." };
            var brand4 = new Brand { BrandId = "BR_GUCCI", BrandName = "Gucci", Description = "Italian luxury fashion house based in Florence, Italy." };
            var brand5 = new Brand { BrandId = "BR_HERMES", BrandName = "Hermès", Description = "French luxury goods manufacturer established in 1837." };

            context.Brands.AddRange(brand1, brand2, brand3, brand4, brand5);

            var products = new Product[]
            {
                new Product 
                { 
                    ProductId = "PROD_LV_NEVERFULL", 
                    ProductName = "Neverfull MM Monogram", 
                    SlugName = "lv-neverfull-mm", 
                    Price = 2030.00m, 
                    StockQuantity = 45, 
                    Description = "The Neverfull MM tote bag unites timeless design with heritage details. Crafted from elegant Monogram canvas with natural cowhide trim, it is ultra-roomy yet never bulky, with side laces that cinch for a sleek allure or loosen for a casual look.", 
                    Image = "https://images.unsplash.com/photo-1584917865442-de89df76afd3?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_BAG", 
                    BrandId = "BR_LV" 
                },
                new Product 
                { 
                    ProductId = "PROD_DIOR_LADY", 
                    ProductName = "Lady Dior Bag Black Lambskin", 
                    SlugName = "lady-dior-black-lambskin", 
                    Price = 6500.00m, 
                    StockQuantity = 15, 
                    Description = "The Lady Dior handbag embodies the House's vision of elegance and beauty. Refined and sleek, the timeless style is crafted in black lambskin with Cannage stitching, creating an instantly recognizable quilted texture.", 
                    Image = "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_BAG", 
                    BrandId = "BR_DIOR" 
                },
                new Product 
                { 
                    ProductId = "PROD_CHANEL_TWEED", 
                    ProductName = "Chanel Classic Tweed Jacket", 
                    SlugName = "chanel-tweed-jacket", 
                    Price = 8200.00m, 
                    StockQuantity = 8, 
                    Description = "An absolute icon of luxury fashion, this classic Chanel Tweed Jacket features structured shoulders, golden signature CC logo buttons, and raw fringe edges. Exquisitely tailored and fully lined in pure silk.", 
                    Image = "https://images.unsplash.com/photo-1591047139829-d91aecb6caea?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_RTW", 
                    BrandId = "BR_CHANEL" 
                },
                new Product 
                { 
                    ProductId = "PROD_GUCCI_BELT", 
                    ProductName = "Double G Leather Belt", 
                    SlugName = "gucci-double-g-belt", 
                    Price = 490.00m, 
                    StockQuantity = 120, 
                    Description = "A hallmark accessory from the House of Gucci, this Double G belt is crafted from smooth black leather and completed with the signature brass interlocking G buckle in a brilliant gold-tone finish.", 
                    Image = "https://images.unsplash.com/photo-1624224971170-2f84fed5eb5e?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_ACC", 
                    BrandId = "BR_GUCCI" 
                },
                new Product 
                { 
                    ProductId = "PROD_HERMES_ORAN", 
                    ProductName = "Hermes Oran Sandal Gold", 
                    SlugName = "hermes-oran-sandal-gold", 
                    Price = 760.00m, 
                    StockQuantity = 60, 
                    Description = "The Hermès Oran sandal is an essential, iconic style. Crafted in Epsom calfskin with the signature 'H' cut-out silhouette. Extremely comfortable and versatile for warm-weather luxury.", 
                    Image = "https://images.unsplash.com/photo-1603808033192-082d6f74b30d?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_SHOE", 
                    BrandId = "BR_HERMES" 
                },
                new Product 
                { 
                    ProductId = "PROD_DIOR_SLINGBACK", 
                    ProductName = "J'Adior Slingback Pump", 
                    SlugName = "dior-j-adior-slingback", 
                    Price = 1150.00m, 
                    StockQuantity = 22, 
                    Description = "A magnificent showcase of Dior savoir-faire, the J'Adior slingback pump is crafted in black technical fabric. The two-tone embroidered 'J'Adior' ribbon is flat-bow finished and sits on a 6.5 cm comma heel.", 
                    Image = "https://images.unsplash.com/photo-1543163521-1bf539c55dd2?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_SHOE", 
                    BrandId = "BR_DIOR" 
                },
                new Product 
                { 
                    ProductId = "PROD_LV_LUGGAGE", 
                    ProductName = "Horizon 55 Monogram Luggage", 
                    SlugName = "lv-horizon-55-luggage", 
                    Price = 3400.00m, 
                    StockQuantity = 10, 
                    Description = "Designed by Marc Newson, widely acknowledged as the most influential industrial designer of his generation, this lightweight 4-wheeled cabin suitcase features a flat interior structure and standard carry-on sizing.", 
                    Image = "https://images.unsplash.com/photo-1563911302283-d2bc129e7570?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_ACC", 
                    BrandId = "BR_LV" 
                },
                new Product 
                { 
                    ProductId = "PROD_GUCCI_SNEAKER", 
                    ProductName = "Gucci Ace Leather Sneaker", 
                    SlugName = "gucci-ace-leather-sneaker", 
                    Price = 790.00m, 
                    StockQuantity = 75, 
                    Description = "The retro-inspired Gucci Ace low-top sneaker features the signature green and red Web stripe. Crafted from white leather with metallic red leather on the back of one shoe and green on the other.", 
                    Image = "https://images.unsplash.com/photo-1608231387042-66d1773070a5?auto=format&fit=crop&w=800&q=80", 
                    CategoryId = "CAT_SHOE", 
                    BrandId = "BR_GUCCI" 
                }
            };

            // Seed coupons
            var coupon1 = new Coupon { CouponId = "COUP_WELCOME", CouponCode = "WELCOME10", DiscountType = "percentage", DiscountAmount = 10m, ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)), Quantity = 100, IsActive = true };
            var coupon2 = new Coupon { CouponId = "COUP_LUX", CouponCode = "LUXURY100", DiscountType = "fixed", DiscountAmount = 100m, MinOrderValue = 500m, ExpiryDate = DateOnly.FromDateTime(DateTime.Now.AddDays(30)), Quantity = 50, IsActive = true };
            context.Coupons.AddRange(coupon1, coupon2);

            context.Products.AddRange(products);
            context.SaveChanges();
        }
    }
}
