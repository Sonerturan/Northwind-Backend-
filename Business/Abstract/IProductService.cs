using Core.Utilities.Results;
using Entities.Concrete;
using Entities.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace Business.Abstract
{
    public interface IProductService
    {
        IDataResult<List<Product>> GetAll();
        IDataResult<List<Product>> GetAllByCategoryId(int id);
        IDataResult<List<Product>> GetByUnitPrice(decimal min, decimal max);
        IDataResult<List<ProductDetailDto>> GetProductDetails();
        IDataResult<Product> GetById(int productId);
        IResult Add(Product product);
        IResult Update(Product product);

        //Transaction yönetimi : uygulamalarda  tutarlılığı korumak için yapılan bir yöntemdir .
        //Örneğin: hesaplar arası para transferinde aynı süreçte iki veri tabanı işlemi var biri azalırken biri artacak
        //gönderirken güncelledi para eksildi fakat alıcı hesapta hata olursa güncellemezse gönderen hesapta iade olması lazım işlem geri alınmalıdır
        IResult AddTransactionalTest(Product product);
    }
}
