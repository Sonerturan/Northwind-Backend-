using Business.Concrete;
using DataAccess.Concrete.EntityFramework;
using DataAccess.Concrete.InMemory;
using Entities.Concrete;
using System;

namespace ConsoleUI
{
    class Program
    {
        static void Main(string[] args)
        {
            ProductTest();
            //CategoryTest();
        }

        private static void CategoryTest()
        {
            CategoryManager categoryManager = new CategoryManager(new EfCategoryDal());
            Console.WriteLine("------------------------\nGetAll()\n------------------------");
            foreach (var category in categoryManager.GetAll().Data)
            {
                Console.WriteLine(category.CategoryName);
            }
            Console.WriteLine("------------------------\nGetById(2)\n------------------------");
            Console.WriteLine(categoryManager.GetById(2).Data.CategoryName);
        }

        private static void ProductTest()
        {
            ProductManager productManager = new ProductManager(new EfProductDal(),new CategoryManager(new EfCategoryDal()));
            Console.WriteLine("------------------------\nGetProductDetails()\n------------------------");
            var result = productManager.GetProductDetails();
            if (result.Success==true)
            {
                foreach (var product in result.Data)
                {
                    //DTO(databse den tabloları join yapma işlemidir) ile bu kod sağlanmıştır
                    Console.WriteLine(product.ProductName + " / " + product.CategoryName);
                }
                Console.WriteLine(result.Message);
            }
            else
            {
                Console.WriteLine(result.Message);
            }




            Console.WriteLine("------------------------\nGetByUnitPrice(50,100)\n------------------------");
            foreach (var product in productManager.GetByUnitPrice(50, 100).Data)
            {
                Console.WriteLine(product.ProductName);
            }
            Console.WriteLine("------------------------\nGetAllByCategoryId(2) \n------------------------");
            foreach (var product in productManager.GetAllByCategoryId(2).Data)
            {
                Console.WriteLine(product.ProductName);
            }
            Console.WriteLine("------------------------\nGetAll() \n------------------------");
            foreach (var product in productManager.GetAll().Data)
            {
                Console.WriteLine(product.ProductName);
            }
            Console.WriteLine("------------------------\nGetById(int id)\n------------------------");
            Console.WriteLine("Görüntülemek istediğiniz ProductId 'yi Yazınız");
            int productId = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine(productManager.GetById(productId).Data.ProductName);
            Console.WriteLine("------------------------\nAdd() \n------------------------");
            Console.WriteLine("CategoryId Giriniz");
            int categoryId = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("ProductName Giriniz");
            string productName = Console.ReadLine();
            Console.WriteLine("UnitsInStock Giriniz");
            short unitInStock = Convert.ToInt16(Console.ReadLine());
            Console.WriteLine("UnitPriceGiriniz");
            decimal unitPrice = Convert.ToInt32(Console.ReadLine());
            productManager.Add(new Product {CategoryId= categoryId, ProductName= productName, UnitsInStock= unitInStock, UnitPrice= unitPrice });
        }
    }
}
