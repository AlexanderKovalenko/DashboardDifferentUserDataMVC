using System.Data.Entity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;

public class Category {
    public Category() {
        Products = new List<Product>();
    }
    [Key]
    public int CategoryID { get; set; }
    public string CategoryName { get; set; }
    public virtual ICollection<Product> Products { get; set; }
}

public class Product {
    [Key]
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public decimal? UnitPrice { get; set; }
    public short? UnitsOnOrder { get; set; }
    public int? CategoryID { get; set; }
    [ForeignKey("CategoryID")]
    public virtual Category Category { get; set; }
}

public class NorthwindDbContext : DbContext {
    public NorthwindDbContext() {
        var userName = (string)HttpContext.Current.Session["CurrentUser"];

        if (userName == "Admin") {
            Database.Connection.ConnectionString = "data source=.;Initial Catalog=CustomNorthwindDB;integrated security=True";
        }
        else if (userName == "User") {
            Database.Connection.ConnectionString = "data source=.;Initial Catalog=CustomNorthwindDB2;integrated security=True";
        }

        if (userName == "Admin") {
            Database.SetInitializer(new NorthwindDbContextInitializer());
        }
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
}

public class NorthwindDbContextInitializer : DropCreateDatabaseIfModelChanges<NorthwindDbContext> {
    protected override void Seed(NorthwindDbContext context) {
        IList<Category> defaultCategories = new List<Category>();

        defaultCategories.Add(new Category() { CategoryName = "Beverages" });
        defaultCategories.Add(new Category() { CategoryName = "Condiments" });
        defaultCategories.Add(new Category() { CategoryName = "Confections" });
        defaultCategories.Add(new Category() { CategoryName = "Dairy Products" });
        defaultCategories.Add(new Category() { CategoryName = "Grains/Cereals" });
        defaultCategories.Add(new Category() { CategoryName = "Meat/Poultry" });
        defaultCategories.Add(new Category() { CategoryName = "Produce" });
        defaultCategories.Add(new Category() { CategoryName = "Seafood" });

        foreach (Category defaultCategory in defaultCategories)
            context.Categories.Add(defaultCategory);

        base.Seed(context);
        
        IList<Product> defaultProducts = new List<Product>();

        defaultProducts.Add(new Product() { ProductName = "Chai", UnitPrice = 19, UnitsOnOrder = 0, CategoryID = 1 });
        defaultProducts.Add(new Product() { ProductName = "Chang", UnitPrice = 19, UnitsOnOrder = 40, CategoryID = 1 });
        defaultProducts.Add(new Product() { ProductName = "Aniseed Syrup", UnitPrice = 10, UnitsOnOrder = 70, CategoryID = 2 });

        defaultProducts.Add(new Product() { ProductName = "Chef Anton's Cajun Seasoning", UnitPrice = 22, UnitsOnOrder = 0, CategoryID = 2 });
        defaultProducts.Add(new Product() { ProductName = "Chef Anton's Gumbo Mix", UnitPrice = 21.35m, UnitsOnOrder = 0, CategoryID = 2 });
        defaultProducts.Add(new Product() { ProductName = "Grandma's Boysenberry Spread", UnitPrice = 25, UnitsOnOrder = 0, CategoryID = 2 });

        defaultProducts.Add(new Product() { ProductName = "Uncle Bob's Organic Dried Pears", UnitPrice = 30, UnitsOnOrder = 0, CategoryID = 7 });
        defaultProducts.Add(new Product() { ProductName = "Northwoods Cranberry Sauce", UnitPrice = 40, UnitsOnOrder = 0, CategoryID = 2 });
        defaultProducts.Add(new Product() { ProductName = "Mishi Kobe Niku", UnitPrice = 97, UnitsOnOrder = 0, CategoryID = 6 });

        defaultProducts.Add(new Product() { ProductName = "Ikura", UnitPrice = 31, UnitsOnOrder = 0, CategoryID = 8 });
        defaultProducts.Add(new Product() { ProductName = "Queso Cabrales", UnitPrice = 21, UnitsOnOrder = 30, CategoryID = 4 });
        defaultProducts.Add(new Product() { ProductName = "Queso Manchego La Pastora", UnitPrice = 38, UnitsOnOrder = 0, CategoryID = 4 });

        foreach (Product defaultProduct in defaultProducts)
            context.Products.Add(defaultProduct);

        base.Seed(context);
    }
}
