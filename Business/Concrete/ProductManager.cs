using Business.Abstract;
using Business.BusinessAspects.Autofac;
using Business.Constans;
using Business.ValidationRules.FluentValidation;
using Core.Aspects.AutoFac.Caching;
using Core.Aspects.AutoFac.Performance;
using Core.Aspects.AutoFac.Transaction;
using Core.Aspects.AutoFac.Validation;
using Core.CrossCuttingConcerns.Validation;
using Core.Utilities.Business;
using Core.Utilities.Results;
using DataAccess.Abstract;
using DataAccess.Concrete.InMemory;
using Entities.Concrete;
using Entities.DTOs;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;

namespace Business.Concrete
{
    public class ProductManager : IProductService
    {
        IProductDal _productDal;
        ICategoryService _categoryService;

        public ProductManager(IProductDal productDal,ICategoryService categoryService)
        {
            _productDal = productDal;
            _categoryService = categoryService;
        }

        public ProductManager()
        {

        }

        //Key,value
        [CacheAspect]
        public IDataResult<List<Product>> GetAll()
        {
            //iş kodları
            if (DateTime.Now.Hour==04)
            {
                return new ErrorDataResult<List<Product>>(Messages.MaintenanceTime);
            }
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(),Messages.ProductListed);
        }

        public IDataResult<List<Product>> GetAllByCategoryId(int id)
        {
            return new SuccessDataResult<List<Product>>(_productDal.GetAll(p => p.CategoryId == id));
        }

        [CacheAspect]
        //bu metodun çalışması 5 sn yi geçerse beni uyar
        [PerformanceAspect(15)]
        public IDataResult<Product> GetById(int productId)
        {
            return new SuccessDataResult<Product>(_productDal.Get(p => p.ProductId == productId));
        }

        public IDataResult<List<Product>> GetByUnitPrice(decimal min, decimal max)
        {
            return new SuccessDataResult<List<Product>>( _productDal.GetAll(p => p.UnitPrice >= min && p.UnitPrice <= max));
        }

        public IDataResult<List<ProductDetailDto>> GetProductDetails()
        {
            if (DateTime.Now.Hour == 02)
            {
                return new ErrorDataResult<List<ProductDetailDto>>(_productDal.GetProductDetails(),Messages.MaintenanceTime);
            }
            return new SuccessDataResult<List<ProductDetailDto>>(_productDal.GetProductDetails(),Messages.ProductListed);
        }

        //claim (yetkilendirme işlemlerine denir)
        [SecuredOperation("product.add,admin")]
        [ValidationAspect(typeof(ProductValidator))]
        [CacheRemoveAspect("IProductService.Get")]
        public IResult Add(Product product)
        {
            //valitadion codes 

            //validation kodlarını validaton rules in içine aldık
            //if (product.ProductName.Length < 2)
            //{
            //    return new ErrorResult(Messages.ProductNameInvalid);
            //}

            //var context = new ValidationContext<Product>(product);
            //ProductValidator productValidator = new ProductValidator();
            //var result = productValidator.Validate(context);
            //if (!result.IsValid)
            //{
            //    throw new ValidationException(result.Errors);
            //}

            //bu kod ile core katmanında yazılan kodları evrensel ve base hale getiriyoruz
            //ValidationTool.Validate(new ProductValidator(), product);
            //aspects ile attribute ekleyerek bu koddan da kurtuluyoruz

            //business codes

            //'bir kategoride en fazla 10 ürün olabilir'? kodu nasıl yazılır?
            //var result = _productDal.GetAll(p=>p.CategoryId==product.CategoryId).Count;
            //if (result>=10)
            //{
            //    return new ErrorResult(Messages.ProductCountOfCategoryError);
            //}
            //private olarak kendimizi tekrar etmeemk için diğer update gibi metotlarda da olacağı için method halinde yazıyoruz

            //harici methodumuzu kullanıyoruz

            //if (CheckIfProductCountOfCategoryCorrect(product.CategoryId).Succes)
            //{
            //    if (CheckIfProductExists(product.ProductName).Succes)
            //    {
            //        _productDal.Add(product);
            //        return new SuccesResult(Messages.ProductAdded);
            //        //return new SuccesResult(); //mesaj vermeden de kullanılabilir
            //    }

            //}
            //return new ErrorResult();

            //burdaki iç içe if karmaşıklığından kurtulup clean code için core.utilities.business.businessrules e gönderip orda çalıştıracağız

            IResult result=BusinessRules.Run(CheckIfProductExists(product.ProductName),
                CheckIfProductCountOfCategoryCorrect(product.CategoryId),
                CheckIfCategoryLimitExceded());

            if (result!=null)
            {
                return result;
            }
            _productDal.Add(product);
            return new SuccessResult(Messages.ProductAdded);

        }

        
        [ValidationAspect(typeof(ProductValidator))]
        //güncelleme olduğunda cache deki veriyi sileriz çünkü değişmiştir yeniden kullanuılırsa yeni hali cache lensin diye
        [CacheRemoveAspect("IProductService.Get")]
        public IResult Update(Product product)
        {
            var result = _productDal.GetAll(p => p.CategoryId == product.CategoryId).Count;
            if (result >= 10)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }
            throw new NotImplementedException();
        }
        //'bir kategoride en fazla 10 ürün olabilir'? kodu nasıl yazılır?
        private IResult CheckIfProductCountOfCategoryCorrect(int categoryId)
        {
            //select (*) from products Where categoryid=1  alttaki yazılı kod bunu çalıştırır
            var result = _productDal.GetAll(p => p.CategoryId == categoryId).Count;
            if (result >= 10)
            {
                return new ErrorResult(Messages.ProductCountOfCategoryError);
            }
            return new SuccessResult();
        }
        //aynı isimde ürün eklenemez kodu nasıl yazılır
        private IResult CheckIfProductExists(string productName)
        {
            var result = _productDal.GetAll(p => p.ProductName == productName).Any();
            if (result)
            {
                return new ErrorResult(Messages.ProductNameAlreadyExists);
            }
            return new SuccessResult();
        }
        //mevcut kategori sayısı 15i geçtiyse sisteme yeni ürün eklenemez kodu nasıl yazılır
        private IResult CheckIfCategoryLimitExceded()
        {
            var result = _categoryService.GetAll();
            if (result.Data.Count>15)
            {
                return new ErrorResult(Messages.CheckIfCategoryLimitExceded);
            }
            return new SuccessResult();
        }

        [TransactionScopeAspect]
        public IResult AddTransactionalTest(Product product)
        {

            //using (TransactionScope scope=new TransactionScope())
            //{
            //    try
            //    {
            //        Add(product);
            //        if (product.UnitPrice<10)
            //        {
            //            throw new Exception("");
            //        }
            //        Add(product);
            //        scope.Complete();
            //    }
            //    catch (Exception)
            //    {

            //        scope.Dispose();
            //    }
            //}
            //return null;

            //TransactionScope işlemi ile transaction yönetimi sağlanabilir ama bunu Attribute ile daha düzgün yapacağız

            Add(product);
            if (product.UnitPrice < 10)
            {
                throw new Exception("aaa");
            }
            Add(product);

            return null;
        }
    }
}
